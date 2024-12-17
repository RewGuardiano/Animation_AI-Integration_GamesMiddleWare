using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using UnityEngine.Events;

public class ChatGPTManager : MonoBehaviour
{
    public onResponseEvent onResponse;

    [System.Serializable]
    public class onResponseEvent: UnityEvent<string>
    {

    }


    // Needed to hardcode API Key and Organization ID since configuration file is not working. 
    private string apiKey = " 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";
    private string organizationId = "org-gYuhgIba3vvel2lmZsBZwwGl";


    private OpenAIApi openAI;

    void Awake()
    {
        // Configure OpenAIApi with the API key and Organization ID
        openAI = new OpenAIApi(apiKey, organizationId);
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
