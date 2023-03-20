using UnityEngine;

[CreateAssetMenu(fileName = "ChatGPT", menuName = "ChatGPT/ChatGPTSettingsScript")]
public class ChatGPTSettingsScript  : ScriptableObject
{
    public string apiURL;

    public string apiKey;

    public string apiOrganization;

    public string apiModel;

    public bool debug;
}