using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.Events;
using System.IO;

public class ChatGPTManager : MonoBehaviour
{
    public onResponseEvent onResponse;

    [System.Serializable]
    public class onResponseEvent: UnityEvent<string>
    {

    }
    private string apiKey; 
    private string organizationId;


    private OpenAIApi openAI;

    void Awake()
    {
        LoadConfiguration();

        // Configure OpenAIApi with the loaded API key and Organization ID
        openAI = new OpenAIApi(apiKey, organizationId);
    }


    private void LoadConfiguration()
    {
        string configPath = "C:/Users/Work/.openai/auth.json"; 

        if (!File.Exists(configPath))
        {
            Debug.LogError("Configuration file not found at: " + configPath);
            return;
        }

        string configJson = File.ReadAllText(configPath);
        Configuration config = JsonUtility.FromJson<Configuration>(configJson);

        apiKey = config.apiKey;
        organizationId = config.organizationId;

    }

    [System.Serializable]
    private class Configuration
    {
        public string apiKey;
        public string organizationId;
    }


    private List<ChatMessage> messages = new List<ChatMessage>();
    public async void AskChatGPT(string newText)
    {
        ChatMessage newMessage = new ChatMessage();
        newMessage.Content = newText;
        newMessage.Role = "user";

        messages.Add(newMessage);


        CreateChatCompletionRequest request = new CreateChatCompletionRequest();
        request.Messages = messages;
        request.Model = "gpt-3.5-turbo";

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            var chatResponse = response.Choices[0].Message;
            messages.Add(chatResponse);

            Debug.Log(chatResponse.Content);

            onResponse.Invoke(chatResponse.Content);

        }

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
