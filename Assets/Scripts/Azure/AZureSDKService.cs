using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AZureSDKService : IService
{

    private AZureSDKService _service = null;
    
    public AZureSDKService(AzureSDKSettings azureSDKSettings)
    {
        SetAzureSettings(azureSDKSettings);
    }
    
    public static AzureSDKSettings AzureSettings { get; private set; }

    /// <summary>
    /// 线程锁
    /// thread locker
    /// </summary>
    public static object threadLocker = new object();
    
    private MicRecognizer micRecognizer;
    private Dictionary<int, AzureSpeecher> speecherDic = new Dictionary<int, AzureSpeecher>();

    private void SetAzureSettings(AzureSDKSettings azureSDKSettings)
    {
        AzureSettings = azureSDKSettings;
    }

    public void SetOnlyOneMicRecognizer(MicRecognizer micRecognizer)
    {
        if (this.micRecognizer)
        {
            Debug.LogError("MicRecognizer repeat set!");
        }
        this.micRecognizer = micRecognizer;
    }
    
    public MicRecognizer GetMicRecognizer()
    {
        return micRecognizer;
    }

    public AzureSpeecher GetSpeecherById(int id)
    {
        if (!speecherDic.ContainsKey(id))
        {
            Debug.LogError("Speecher not exist!");
            return null;
        }
        return speecherDic[id];
    }
    
    public void SetSpeecherById(int id, AzureSpeecher speecher)
    {
        if (speecherDic.ContainsKey(id))
        {
            Debug.LogError($"Speecher id {id} not set!");
            return;
        }
        speecherDic[id] = speecher;
    }

    public T Register<T>() where T : IService
    {
        return (T) (object)this;
    }

    public T Get<T>() where T : IService
    {
        return (T) (object) this;
    }
}
