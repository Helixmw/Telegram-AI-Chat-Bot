using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;
using MWBotApp.Exceptions;
using MWBotApp.Models;
using MWBotApp.Utilities;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;



namespace MWBotApp.Logic;
public partial class TelegramBotProcessor : BaseProcessor
{

    public TelegramBotClient Bot
    {
        get; private set;
    }

    private readonly AIChatClient aIChatClient;
    private readonly AIVoiceChatClient aIVoiceChatClient;
    private long chatId;
    
    public readonly ReceiverOptions ReceiverOptions = new()
    {
        AllowedUpdates = {}
    };
    public readonly CancellationTokenSource tokenSource = new();

    public TelegramBotProcessor(TelegramBotClient bot, 
        AIChatClient aIChatClient,
        AIVoiceChatClient aIVoiceChatClient)
    {
        Bot = bot;
        this.aIChatClient = aIChatClient;
        this.aIVoiceChatClient = aIVoiceChatClient;     
    }
  

    //Handles application errors and gives error output
    public void HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        var parameters = new MessageResponseParameters
        {
            BotClient = (TelegramBotClient)botClient,
            Exception = exception,
            CancellationToken = cancellationToken
        };
        ThreadPool.QueueUserWorkItem(async state => await StartErrorMessageResponse(parameters));
    }

    //Handles message updates from the user and responds
    public void HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        var _params = new MessageResponseParameters
        {
            BotClient = (TelegramBotClient)botClient,
            Update = update,
            CancellationToken = cancellationToken
        };
        ThreadPool.QueueUserWorkItem(async state => await StartMessageResponse(_params));
    }

  
}
