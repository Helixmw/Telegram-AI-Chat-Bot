using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MWBotApp.Helpers;
using OpenAI.Audio;
using OpenAI.Chat;

namespace MWBotApp.Utilities;

//Base class for all AI related tasks (Chat, Voice etc.)
public abstract class AIClient
{
    
    protected IConfiguration? _configuration;

    protected string apiKey = string.Empty;
    protected string model;
    protected string systemPromptMessage = string.Empty;
    protected ChatClient? chatClient;
    protected AudioClient? audioClient;
    protected readonly List<ChatMessage> messages = new();
    protected AsyncCollectionResult<StreamingChatCompletionUpdate>? completionUpdates;
    protected readonly string ErrorMessage;
    
    //ChatTools used in MessageCompletion by the API
    protected static readonly ChatTool getCurrentLocationTool = ChatTool.CreateFunctionTool(
            functionName: nameof(AIChatTools.GiveLocation),
            functionDescription: "Get's the users current location"
        );

    //Set Completion options with the tools to be used in the Message Completion
    protected readonly ChatCompletionOptions chatCompletionOptions = new()
    {
        Tools = { getCurrentLocationTool }
    };

    protected AIClient(string model)
    {
        _configuration = SetConfigurationBuilder();
        ErrorMessage = _configuration.GetSection("Administrator:DefaultErrorMessage").Value ?? "Something went wrong. Please try again later";
        //Gets OpenAI API Key
        apiKey = _configuration.GetSection("ApiKeys:OpenAIKey").Value ?? string.Empty;
        //System Prompt that tells the AI what its tasks are
        systemPromptMessage = _configuration.GetSection("AISystemPrompt").Value ?? string.Empty;
        this.model = model;
 
    }

    private IConfiguration SetConfigurationBuilder()
    {
      return new ConfigurationBuilder().AddUserSecrets<AIClient>().Build();
    }

    /*Set up chat client and voice clients*/

    protected void SetChatClient()
    {
        chatClient = new(model, apiKey);
    }


    protected void SetAudioClient()
    {
        audioClient = new(model, apiKey);
        
    }

}
