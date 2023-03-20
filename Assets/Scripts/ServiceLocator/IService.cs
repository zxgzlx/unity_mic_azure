public interface IService
{
    /// <summary>
    /// 唯一一次注册服务器
    /// register service only once
    /// </summary>
    /// <param name="service"></param>
    /// <typeparam name="T"></typeparam>
    T Register<T>() where T : IService;
    /// <summary>
    /// 获取服务器单例
    /// get service singleton
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T Get<T>() where T : IService;
}