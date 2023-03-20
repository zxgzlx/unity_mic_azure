using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator : IServiceLocator
{
    private static ServiceLocator _instance;
    public static ServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ServiceLocator();
            }
            return _instance;
        }
    }
    
    // 服务器容器
    // Service Container
    private Dictionary<string, IService> _serviceContainer = new Dictionary<string, IService>();

    public void Init()
    {
        RegisterService();
    }

    /// <summary>
    /// 初始化时候统一注册服务器
    /// Initialize the server uniformly when registering
    /// </summary>
    private void RegisterService()
    {
        
    }
    
    public void RegisterService<T>(T service) where T : IService
    {
        AddService(service);
    }

    private void AddService<T>(T service) where T : IService
    {
        string serviceName = typeof(T).Name;
        Debug.Log("service name: " + serviceName);
        if (_serviceContainer.ContainsKey(serviceName))
        {
            throw new Exception($"{serviceName}service repeat register！");
        }
        _serviceContainer.Add(serviceName, service);
    }
    
    public T GetService<T>() where T : IService
    {
        if (!_serviceContainer.ContainsKey(typeof(T).Name))
        {
            throw new Exception($"{typeof(T).Name} service not register！");
        }
        return _serviceContainer[typeof(T).Name].Get<T>();
    }
    
    public void RemoveService<T>() where T : IService
    {
        if (!_serviceContainer.ContainsKey(typeof(T).Name))
        {
            throw new Exception($"{typeof(T).Name} service not register！");
        }
        _serviceContainer.Remove(typeof(T).Name);
    }
    
    public void RemoveAllService()
    {
        _serviceContainer.Clear();
    }
}