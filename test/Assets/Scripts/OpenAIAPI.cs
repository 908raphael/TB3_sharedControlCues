using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;

public class OpenAIAPI : MonoBehaviour
{
    private string apiUrl = "https://api.openai.com/v1/chat/completions";
    private string apiKey = "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"; // 替换为您的 API 密钥
    private const float DEFAULT_TIMEOUT = 30f;

    [SerializeField]
    private float requestTimeout = DEFAULT_TIMEOUT;

    [Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [Serializable]
    public class OpenAIRequest
    {
        public string model = "gpt-4";
        public Message[] messages;
        public float temperature = 0.7f;
        public int max_tokens = 2000;
    }

    [Serializable]
    public class Choice
    {
        public Message message;
        public string finish_reason;
    }

    [Serializable]
    public class Error
    {
        public string message;
        public string type;
        public string code;
    }

    [Serializable]
    public class OpenAIResponse
    {
        public string id;
        public string object_type;
        public long created;
        public Choice[] choices;
        public Error error;
    }

    // 发送消息并获取回复
    void Start()
    {
        SendMessageToGPT( "你好！");
    }
    public void SendMessageToGPT(string userMessage)
    {
        Message[] messages = new Message[]
        {
            new Message { role = "user", content = userMessage }
        };

        OpenAIRequest requestData = new OpenAIRequest
        {
            model = "gpt-4",
            messages = messages
        };

        string jsonData = JsonConvert.SerializeObject(requestData);
        StartCoroutine(SendRequest(jsonData));
    }

    private IEnumerator SendRequest(string jsonData)
    {
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.timeout = Mathf.RoundToInt(requestTimeout);

            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();
            // var operation = request.SendWebRequest();
            // float timer = 0f;

            // 添加超时检查
            // while (!operation.isDone)
            // {
            //     timer += Time.deltaTime;
            //     if (timer >= requestTimeout)
            //     {
            //         request.Abort();
            //         onError?.Invoke("请求超时");
            //         yield break;
            //     }
            //     yield return null;
            // }

            if (request.result != UnityWebRequest.Result.Success)
            {   
                Debug.Log(request.downloadHandler.text);
                // yield break;
            }

            // try
            // {
            //     OpenAIResponse response = JsonConvert.DeserializeObject<OpenAIResponse>(request.downloadHandler.text);
                
            //     // 检查 API 返回的错误
            //     if (response.error != null)
            //     {
            //         onError?.Invoke(response.error.message);
            //         yield break;
            //     }

            //     // 检查响应是否有效
            //     if (response.choices == null || response.choices.Length == 0)
            //     {
            //         onError?.Invoke("API 返回的响应无效");
            //         yield break;
            //     }

            //     string responseText = response.choices[0].message.content;
            //     onSuccess?.Invoke(responseText);
            // }
            // catch (JsonException e)
            // {
            //     onError?.Invoke($"解析响应失败: {e.Message}");
            // }
            // catch (Exception e)
            // {
            //     onError?.Invoke($"发生未知错误: {e.Message}");
            // }
        }
    }

}