using System;
using System.IO;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MicRecognizer : MonoBehaviour
{
    /// <summary>
    /// 监听Mic的声音源
    /// listener of the Mic
    /// </summary>
    [SerializeField] private AudioSource _MicAudioSource;
    /// <summary>
    /// 声音识别成文本的输出
    /// speech recognition output
    /// </summary>
    [SerializeField] private Text outputText;
    /// <summary>
    /// 开始声音识别文本监听按钮
    /// start speech recognition button
    /// </summary>
    [SerializeField] private LongHoldBtn recoButton;
    /// <summary>
    /// ChatGTP的设置
    /// </summary>
    [SerializeField] private ChatGPTSettingsScript chatGptSettingsScript;
    /// <summary>
    /// azure sdk的设置
    /// </summary>
    [SerializeField] private AzureSDKSettings azureSDKSettings;

    /// <summary>
    /// 是否已经获取到麦克风权限
    /// is mic permission granted
    /// </summary>
    private bool micPermissionGranted = false;
    /// <summary>
    /// 声音输入流
    /// speech input stream
    /// </summary>
    private SpeechRecognizer recognizer;
    /// <summary>
    /// 语音sdk配置
    /// speech sdk config
    /// </summary>
    private SpeechConfig config;
    /// <summary>
    /// 声音输入配置
    /// audio input config
    /// </summary>
    private AudioConfig audioInput;
    /// <summary>
    /// 声音输入流
    /// audio input stream
    /// </summary>
    private PushAudioInputStream pushStream;
    /// <summary>
    /// 声音转换成文本的消息内容
    /// audio to text message
    /// </summary>
    private string message;
    /// <summary>
    /// 正在声音识别文本
    /// text recognition started
    /// </summary>
    private bool recognitionStarted = false;
    /// <summary>
    /// 采样率
    /// simple rate
    /// </summary>
    int lastSample = 0;

    /// <summary>
    /// 输出的文本消息
    /// output text message
    /// </summary>
    public string OutTextMessage { get; private set; }
    
    private AZureSDKService _aZureSDKService;

    private void Awake()
    {
        ServiceLocator.Instance.Init();
        _aZureSDKService = new AZureSDKService(azureSDKSettings);
        ServiceLocator.Instance.RegisterService(_aZureSDKService);
        _aZureSDKService.SetOnlyOneMicRecognizer(this);
    }

    void Start()
    {

        if (outputText == null)
        {
            Debug.LogError("outputText property is null! Assign a UI Text element to it.");
        }
        else if (recoButton == null)
        {
            message = "recoButton property is null! Assign a UI Button to it.";
            Debug.LogError(message);
        }
        else
        {
            // Continue with normal initialization, Text and Button objects are present.
#if PLATFORM_ANDROID
            // Request to use the microphone, cf.
            // https://docs.unity3d.com/Manual/android-RequestingPermissions.html
            message = "Waiting for mic permission";
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Permission.RequestUserPermission(Permission.Microphone);
            }
#elif PLATFORM_IOS
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Application.RequestUserAuthorization(UserAuthorization.Microphone);
            }
#else
            micPermissionGranted = true;
            message = "Click button to recognize speech";
#endif
            config = SpeechConfig.FromSubscription(AZureSDKService.AzureSettings.YourSubscriptionKey, AZureSDKService.AzureSettings.YourServiceRegion);
            // config.SpeechRecognitionLanguage = "zh-CN"; // 更改源语言识别，不然识别默认的是英语，不正确
            config.SpeechRecognitionLanguage = AZureSDKService.AzureSettings.SpeechRecognitionLanguage;
            
            pushStream = AudioInputStream.CreatePushStream();
            audioInput = AudioConfig.FromStreamInput(pushStream);
            recognizer = new SpeechRecognizer(config, audioInput);
            recognizer.Recognizing += RecognizingHandler;
            recognizer.Recognized += RecognizedHandler;
            recognizer.Canceled += CanceledHandler;

            // recoButton.onClick.AddListener(ButtonClick);
            recoButton.OnLongPressStartEvent.AddListener(OnLongPressStart);
            recoButton.OnLongPressEndEvent.AddListener(OnLongPressEnd);
            foreach (var device in Microphone.devices)
            {
                Debug.Log("DeviceName: " + device);                
            }
        }
    }
    
    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        lock (AZureSDKService.threadLocker)
        {
            message = e.Result.Text;
            Debug.Log("RecognizingHandler: " + message);
        }
    }
    
    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        lock (AZureSDKService.threadLocker)
        {
            message = e.Result.Text;
            Debug.Log("RecognizedHandler: " + message);
        }
    }

    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        lock (AZureSDKService.threadLocker)
        {
            message = e.ErrorDetails.ToString();
            Debug.Log("CanceledHandler: " + message);
        }
    }
    
    public async void OnLongPressStart()
    {
        OutTextMessage = String.Empty;
        Debug.Log("OnLongPressStart");
        if (!Microphone.IsRecording(Microphone.devices[0]))
        {
            Debug.Log("Microphone.Start: " + Microphone.devices[0]);
            _MicAudioSource.clip = Microphone.Start(Microphone.devices[0], true, 200, 16000);
            Debug.Log("audioSource.clip channels: " + _MicAudioSource.clip.channels);
            Debug.Log("audioSource.clip frequency: " + _MicAudioSource.clip.frequency);
        }

        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        lock (AZureSDKService.threadLocker)
        {
            recognitionStarted = true;
            Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
        }
    }

    public async void OnLongPressEnd()
    {
        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(true);

        if (Microphone.IsRecording(Microphone.devices[0]))
        {
            Debug.Log("Microphone.End: " + Microphone.devices[0]);
            Microphone.End(null);
            lastSample = 0;
        }

        lock (AZureSDKService.threadLocker)
        {
            recognitionStarted = false;
            OutTextMessage = message;
            Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
            StartCoroutine(HttpTool.Post(OutTextMessage, chatGptSettingsScript, (result) =>
            {
                AzureSpeecher azureSpeecher = _aZureSDKService.GetSpeecherById(0);
                Debug.Log("result: " + result);
                if (result != null)
                {
                    azureSpeecher.Speaking(result.Choices[0].Message.content);
                }
            }));
        }
    }
    
    // public async void ButtonClick()
    // {
    //     OutTextMessage = String.Empty;
    //     if (recognitionStarted)
    //     {
    //         await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(true);
    //
    //         if (Microphone.IsRecording(Microphone.devices[0]))
    //         {
    //             Debug.Log("Microphone.End: " + Microphone.devices[0]);
    //             Microphone.End(null);
    //             lastSample = 0;
    //         }
    //
    //         lock (AZureSDKService.threadLocker)
    //         {
    //             recognitionStarted = false;
    //             OutTextMessage = message;
    //             Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
    //         }
    //     }
    //     else
    //     {
    //         if (!Microphone.IsRecording(Microphone.devices[0]))
    //         {
    //             Debug.Log("Microphone.Start: " + Microphone.devices[0]);
    //             _MicAudioSource.clip = Microphone.Start(Microphone.devices[0], true, 200, 16000);
    //             Debug.Log("audioSource.clip channels: " + _MicAudioSource.clip.channels);
    //             Debug.Log("audioSource.clip frequency: " + _MicAudioSource.clip.frequency);
    //         }
    //
    //         await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
    //         lock (AZureSDKService.threadLocker)
    //         {
    //             recognitionStarted = true;
    //             Debug.Log("RecognitionStarted: " + recognitionStarted.ToString());
    //         }
    //     }
    // }

    private byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
    {
        MemoryStream dataStream = new MemoryStream();
        int x = sizeof(Int16);
        Int16 maxValue = Int16.MaxValue;
        int i = 0;
        while (i < data.Length)
        {
            dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
            ++i;
        }
        byte[] bytes = dataStream.ToArray();
        dataStream.Dispose();
        return bytes;
    }
    
    void FixedUpdate()
    {
#if PLATFORM_ANDROID
        if (!micPermissionGranted && Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            micPermissionGranted = true;
            message = "Click button to recognize speech";
        }
#elif PLATFORM_IOS
        if (!micPermissionGranted && Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            micPermissionGranted = true;
            message = "Click button to recognize speech";
        }
#endif
        lock (AZureSDKService.threadLocker)
        {
            if (recoButton != null)
            {
                recoButton.interactable = micPermissionGranted;
            }
            if (outputText != null)
            {
                outputText.text = message;
            }
        }

        if (Microphone.IsRecording(Microphone.devices[0]) && recognitionStarted == true)
        {
            recoButton.GetComponentInChildren<Text>().text = "Stop";
            int pos = Microphone.GetPosition(Microphone.devices[0]);
            int diff = pos - lastSample;

            if (diff > 0)
            {
                float[] samples = new float[diff * _MicAudioSource.clip.channels];
                _MicAudioSource.clip.GetData(samples, lastSample);
                byte[] ba = ConvertAudioClipDataToInt16ByteArray(samples);
                if (ba.Length != 0)
                {
                    Debug.Log("pushStream.Write pos:" + Microphone.GetPosition(Microphone.devices[0]).ToString() + " length: " + ba.Length.ToString());
                    pushStream.Write(ba);
                }
            }
            lastSample = pos;
        }
        else if (!Microphone.IsRecording(Microphone.devices[0]) && recognitionStarted == false)
        {
            recoButton.GetComponentInChildren<Text>().text = "Start";
        }
    }

    private void OnDestroy()
    {
        recognizer.Recognizing -= RecognizingHandler;
        recognizer.Recognized -= RecognizedHandler;
        recognizer.Canceled -= CanceledHandler;
        pushStream.Close();
        recognizer.Dispose();
    }
}