public interface IServiceLocator
{
    /// <summary>
    /// 定位器初始化,只初始化一次
    /// service locator init, only init once
    /// </summary>
    void Init();
    /// <summary>
    /// 手动注册服务器
    /// manual register service
    /// </summary>
    /// <param name="service"></param>
    /// <typeparam name="T"></typeparam>
    void RegisterService<T>(T service) where T : IService;
    /// <summary>
    /// 获取已注册的某个服务器
    /// get registered service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T GetService<T>() where T : IService;
    /// <summary>
    /// 移除已注册的某个服务器
    /// remove registered service
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void RemoveService<T>() where T : IService;
    /// <summary>
    /// 移除所有的服务器
    /// remove all service
    /// </summary>
    void RemoveAllService();
}