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
    
    //ChatTools
    protected static readonly ChatTool getCurrentLocationTool = ChatTool.CreateFunctionTool(
            functionName: nameof(AIChatTools.GiveLocation),
            functionDescription: "Get's the users current location"
        );

    //Set Completion options with tools
    protected readonly ChatCompletionOptions chatCompletionOptions = new()
    {
        Tools = { getCurrentLocationTool }
    };

    protected AIClient(string model)
    {
        _configuration = SetConfigurationBuilder();
        apiKey = _configuration.GetSection("ApiKeys:OpenAIKey").Value ?? string.Empty;
        systemPromptMessage = _configuration.GetSection("AISystemPrompt").Value ?? string.Empty;
        this.model = model;
 
    }

    private IConfiguration SetConfigurationBuilder()
    {
      return new ConfigurationBuilder().AddUserSecrets<AIClient>().Build();
    }

    protected void SetChatClient()
    {
        chatClient = new(model, apiKey);
    }

    protected void SetAudioClient()
    {
        audioClient = new(model, apiKey);
        
    }

}
