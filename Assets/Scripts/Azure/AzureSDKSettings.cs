using UnityEngine;

[CreateAssetMenu(fileName = "Azure", menuName = "Azure/AzureSDKSettings")]
public class AzureSDKSettings : ScriptableObject
{
    public string YourSubscriptionKey;
    public string YourServiceRegion;
    public string SpeechRecognitionLanguage;
    public string SpeechSynthesisLanguage;
    public string SpeechSynthesisVoiceName;
}