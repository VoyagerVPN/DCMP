using DCMP.Core.Interfaces;
using System.Collections.Concurrent;

namespace DCMP.Core;

public static class ServiceLocator
{
    private static readonly ConcurrentDictionary<Type, IService> _services = new();

    public static void Register<T>(T service) where T : IService
    {
        if (!_services.TryAdd(typeof(T), service))
        {
            throw new InvalidOperationException($"Service {typeof(T).Name} is already registered.");
        }
        service.Initialize();
    }

    public static T Get<T>() where T : IService
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }
        throw new KeyNotFoundException($"Service {typeof(T).Name} not found.");
    }

    public static void UpdateAll(double dt)
    {
        foreach (var service in _services.Values)
        {
            service.Update(dt);
        }
    }

    public static void DisposeAll()
    {
        foreach (var service in _services.Values)
        {
            service.Dispose();
        }
        _services.Clear();
    }
}
