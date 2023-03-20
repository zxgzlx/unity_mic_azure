using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class HttpTool
{
    public static IEnumerator Get(string url, System.Action<string> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(request.error);
            }
            else
            {
                callback(request.downloadHandler.text);
            }
        }
    }

    public static IEnumerator Post(string prompt, ChatGPTSettingsScript chatGtpSettings, Action<ChatGPTResponse> callback)
    {
        var url = chatGtpSettings.debug ? $"{chatGtpSettings.apiURL}?debug=true" : chatGtpSettings.apiURL;
        Debug.Log("=====================url = " + url + " prompt = " + prompt + "=====================");
        // 这个地方内存泄漏，不知道什么原因，好奇怪，报错如下：
        // A Native Collection has not been disposed, resulting in a memory leak. Allocated from:
        // Unity.Collections.NativeArray`1:.ctor(Byte[], Allocator)
        // UnityEngine.Networking.UploadHandlerRaw:.ctor(Byte[])
        // UnityEngine.Networking.UnityWebRequest:SetupPost(UnityWebRequest, String)
        // 解决如下，不要使用UnityWebRequest.Post(url, "POST")这个方法
        // 改用UnityWebRequest request = new UnityWebRequest(url), request.method = UnityWebRequest.kHttpVerbPOST;方法即可解决内存泄漏
        using (UnityWebRequest request = new UnityWebRequest(url))
        {
            request.method = UnityWebRequest.kHttpVerbPOST;
            var requestStartDateTime = DateTime.Now;
            var requestParams = JsonConvert.SerializeObject(new ChatGPTRequest
            {
                Model = chatGtpSettings.apiModel,
                Messages = new ChatGPTChatMessage[]
                {
                    new ChatGPTChatMessage
                    {
                        role = "user",
                        content = prompt
                    }
                }
            });
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestParams);
            using (var uploadHandler = new UploadHandlerRaw(bodyRaw))
            {
                request.uploadHandler = uploadHandler;
                request.downloadHandler = new DownloadHandlerBuffer();
        
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", $"Bearer {chatGtpSettings.apiKey}");
                // request.SetRequestHeader("OpenAI-Organization", chatGTPSettings.apiOrganization);
                request.disposeDownloadHandlerOnDispose = true;
                request.disposeUploadHandlerOnDispose = true;
                request.disposeCertificateHandlerOnDispose = true;
                
                yield return request.SendWebRequest();
        
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(request.error);
                }
                else
                {
                    string responseInfo = request.downloadHandler.text;
                    var response = JsonConvert.DeserializeObject<ChatGPTResponse>(responseInfo);
        
                    response.ResponseTotalTime = (DateTime.Now - requestStartDateTime).TotalMilliseconds;
        
                    callback(response);
                }
            }
        }
    }
}