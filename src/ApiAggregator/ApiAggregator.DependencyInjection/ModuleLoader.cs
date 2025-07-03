using System.Reflection;

namespace ApiAggregator.DependencyInjection;

public static class ModuleLoader
{
    public static List<IModule> LoadAll()
    {
        //Find all solution assemblies.
        var assemblyPaths = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");

        //Filter out all third party assemblies.
        var assemblyNames = assemblyPaths
            .Select(Path.GetFileNameWithoutExtension)
            .Where(n => n!.StartsWith("ApiAggregator"))
            .ToList();

        //Find all classes that implement IModule.
        return assemblyNames
            .Select(Assembly.Load!)
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IModule).IsAssignableFrom(t) && t.IsClass)
            .Select(t => (IModule)Activator.CreateInstance(t)!)
            .ToList();
    }
}