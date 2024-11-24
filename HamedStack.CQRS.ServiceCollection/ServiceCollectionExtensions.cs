using HamedStack.CQRS.FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;

namespace HamedStack.CQRS.ServiceCollection;

/// <summary>
/// Provides extension methods for registering application-specific services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application-specific services, including validators, MediatR, and command/query dispatchers, to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which services are added.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var commandValidatorsAssemblies = GetAllAppDomainAssemblies()
            .FindAssembliesWithImplementationsOf(typeof(CommandValidator<>));
        var commandValidatorsAssemblies2 = GetAllAppDomainAssemblies()
            .FindAssembliesWithImplementationsOf(typeof(CommandValidator<,>));
        var queryValidatorsAssemblies = GetAllAppDomainAssemblies()
            .FindAssembliesWithImplementationsOf(typeof(QueryValidator<,>));

        var validatorsAssemblies = commandValidatorsAssemblies.Concat(commandValidatorsAssemblies2)
            .Concat(queryValidatorsAssemblies).ToList();
        if (validatorsAssemblies.Any())
        {
            services.AddValidatorsFromAssemblies(validatorsAssemblies);
        }

        var allTypes = new[]
        {
            typeof(IMediator),
            typeof(IQuery<>),
            typeof(ICommand),
            typeof(ICommand<>),
            typeof(IQueryHandler<,>),
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>)

        };

        var appDomain = AppDomain.CurrentDomain.GetAssemblies();

        var assemblies1 = AppDomainContains(allTypes);
        var assemblies2 = appDomain.FindAssembliesWithImplementationsOf(typeof(ICommand));
        var assemblies3 = appDomain.FindAssembliesWithImplementationsOf(typeof(ICommand<>));
        var assemblies4 = appDomain.FindAssembliesWithImplementationsOf(typeof(IQuery<>));

        var assemblies = assemblies1.Concat(assemblies2).Concat(assemblies3).Concat(assemblies4).ToArray();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
        services.AddScoped<ICommandQueryDispatcher, CommandQueryDispatcher>();

        return services;
    }


    /// <summary>
    /// Determines if an assembly contains any of the specified types.
    /// </summary>
    /// <param name="assembly">The <see cref="Assembly"/> to inspect.</param>
    /// <param name="types">The types to search for in the assembly.</param>
    /// <returns><see langword="true"/> if the assembly contains any of the specified types; otherwise, <see langword="false"/>.</returns>
    private static bool Contains(this Assembly assembly, params Type[] types)
    {
        var assemblyTypes = assembly.GetTypes().SelectMany(t => new[] { t }.Concat(t.GetNestedTypes()));
        return types.Any(type => assemblyTypes.Contains(type));
    }

    /// <summary>
    /// Retrieves all assemblies in the current application domain that contain the specified types.
    /// </summary>
    /// <param name="types">The types to search for in the assemblies.</param>
    /// <returns>An enumerable of <see cref="Assembly"/> objects that contain the specified types.</returns>
    private static IEnumerable<Assembly> AppDomainContains(params Type[] types)
    {
        return AppDomain.CurrentDomain.GetAssemblies().Where(a => a.Contains(types));
    }

    /// <summary>
    /// Finds all assemblies in the given collection that contain implementations of a specific type or generic type definition.
    /// </summary>
    /// <param name="assemblies">The collection of assemblies to search.</param>
    /// <param name="targetType">The type or generic type definition to search for.</param>
    /// <returns>An enumerable of <see cref="Assembly"/> objects containing implementations of the specified type.</returns>
    private static IEnumerable<Assembly> FindAssembliesWithImplementationsOf(this IEnumerable<Assembly> assemblies,
        Type targetType)
    {
        var resultAssemblies = new HashSet<Assembly>();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract || type == targetType)
                {
                    continue;
                }

                bool typeMatches;

                if (targetType.IsGenericTypeDefinition)
                {
                    typeMatches = type.GetInterfaces()
                                      .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == targetType) ||
                                  type.BaseType is { IsGenericType: true } &&
                                  type.BaseType.GetGenericTypeDefinition() == targetType;
                }
                else
                {
                    typeMatches = targetType.IsAssignableFrom(type);
                }

                if (!typeMatches) continue;

                resultAssemblies.Add(assembly);
                break;
            }
        }

        return resultAssemblies;
    }

    /// <summary>
    /// Retrieves all assemblies loaded in the current application domain, including those in the base directory.
    /// </summary>
    /// <returns>A list of <see cref="Assembly"/> objects available in the current application domain.</returns>
    private static IEnumerable<Assembly> GetAllAppDomainAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

        var assemblyDir = AppDomain.CurrentDomain.BaseDirectory;

        var allDllFiles = Directory.GetFiles(assemblyDir, "*.dll", SearchOption.AllDirectories);

        foreach (var dllFile in allDllFiles)
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(dllFile);

                if (assemblies.Any(a => a.FullName == assemblyName.FullName)) continue;

                var loadedAssembly = Assembly.Load(assemblyName);
                assemblies.Add(loadedAssembly);
            }
            catch
            {
                // ignored
            }
        }

        return assemblies;
    }
}