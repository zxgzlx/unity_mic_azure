using System;
using System.Threading;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using UnityEngine;
using UnityEngine.UI;

public class AzureSpeecher : MonoBehaviour
{
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public Button speechButton;
    [SerializeField] public Text outputText;

    SpeechConfig config;
    AudioConfig audioInput;
    private SpeechSynthesizer synthesizer;
    
    private string message;
    /// <summary>
    /// 采样率
    /// sample rate
    /// </summary>
    private const int SampleRate = 24000;
    private bool audioSourceNeedStop;
    
    /// <summary>
    /// TODO 
    /// 等待语音播放结束，这个预留给mic交流的对象用，用来区别正在和那个对象交流
    /// waiting for the end of the speech, this is reserved for the mic communication object to use, to distinguish between the object being communicated with
    /// </summary>
    private bool isWaitingSpeecherEcho = false;
    /// <summary>
    /// 管理azure sdk的服务器，相当于sdk管理类
    /// zaure sdk service
    /// </summary>
    private AZureSDKService aZureSdkService;

    private void Start()
    {
        config = SpeechConfig.FromSubscription(AZureSDKService.AzureSettings.YourSubscriptionKey, AZureSDKService.AzureSettings.YourServiceRegion);
        // config.SpeechSynthesisLanguage = "zh-CN";
        // config.SpeechSynthesisVoiceName = "zh-CN-XiaomoNeural";
        config.SpeechSynthesisLanguage = AZureSDKService.AzureSettings.SpeechSynthesisLanguage;
        config.SpeechSynthesisVoiceName = AZureSDKService.AzureSettings.SpeechSynthesisVoiceName;
        config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);

        synthesizer = new SpeechSynthesizer(config, null);

        synthesizer.SynthesisCanceled += (s, e) =>
        {
            isWaitingSpeecherEcho = false;
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
            message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
        };
        
        speechButton.onClick.AddListener(OnClickBtn);
        aZureSdkService = ServiceLocator.Instance.GetService<AZureSDKService>();
        aZureSdkService.SetSpeecherById(0, this);
    }

    private void OnClickBtn()
    {
        // Speaking("你好，我是小莫，你可以叫我小莫，我是你的智能助理，我可以帮你做很多事情，比如帮你记账");
        // Speaking(aZureSdkService.GetMicRecognizer().OutTextMessage);
    }

    public void Speaking(string talkText)
    {
        outputText.text = message = talkText;
        string newMessage = null;
        var startTime = DateTime.Now;

        isWaitingSpeecherEcho = true;
        using (var result = synthesizer.StartSpeakingTextAsync(talkText).Result)
        {
            // Native playback is not supported on Unity yet (currently only supported on Windows/Linux Desktop).
            // Use the Unity API to play audio here as a short term solution.
            // Native playback support will be added in the future release.
            Debug.Log("===================hahah " + result.AudioData.Length);
            var audioDataStream = AudioDataStream.FromResult(result);
            var isFirstAudioChunk = true;
            var audioClip = AudioClip.Create(
                "Speech",
                SampleRate * 600, // Can speak 10mins audio as maximum
                1,
                SampleRate,
                true,
                (float[] audioChunk) =>
                {
                    var chunkSize = audioChunk.Length;
                    var audioChunkBytes = new byte[chunkSize * 2];
                    var readBytes = audioDataStream.ReadData(audioChunkBytes);
                    if (isFirstAudioChunk && readBytes > 0)
                    {
                        var endTime = DateTime.Now;
                        var latency = endTime.Subtract(startTime).TotalMilliseconds;
                        newMessage = $"Speech synthesis succeeded!\nLatency: {latency} ms.";
                        isFirstAudioChunk = false;
                    }

                    for (int i = 0; i < chunkSize; ++i)
                    {
                        if (i < readBytes / 2)
                        {
                            audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) /
                                            32768.0F;
                        }
                        else
                        {
                            audioChunk[i] = 0.0f;
                        }
                    }

                    if (readBytes == 0)
                    {
                        Thread.Sleep(200); // Leave some time for the audioSource to finish playback
                        audioSourceNeedStop = true;
                    }
                });

            audioSource.clip = audioClip;
            audioSource.Play();
            isWaitingSpeecherEcho = false;
        }
    }

    private void OnDestroy()
    {
        if (synthesizer != null)
        {
            synthesizer.Dispose();
            isWaitingSpeecherEcho = false;
        }
    }
}