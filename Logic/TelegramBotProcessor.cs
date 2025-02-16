using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;
using MWBotApp.Utilities;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;



namespace MWBotApp.Logic;
public class TelegramBotProcessor
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

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        await botClient.SendChatAction(chatId, ChatAction.Typing);
        await botClient.SendMessage(chatId,
                      "<b>Whoops!</b>\nSomething went wrong somewhere. Perhaps trying another time?",
                     ParseMode.Html,
                     protectContent: 
                     true);
        Console.WriteLine($"Your Error: {exception.Message}");
    }
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {

        switch (update.Message?.Type)
        {
            case MessageType.Text:
                chatId = update.Message.Chat.Id;
                if (update.Message?.Text?.Trim() != string.Empty && update.Message?.Text is not null)
                {
                    var msg = update.Message.Text.Trim();
                    await botClient.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                    var message = await aIChatClient.SendRequest(msg);
                    await botClient.SendMessage(update.Message.Chat.Id, message);
                }
            break;
            default:

                if(update.Message is not null)
                {
                    chatId = update.Message.Chat.Id;
                    await botClient.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                    var me = await botClient.GetMe();
                    await botClient.SendMessage(update.Message.Chat.Id,
                    $"<b>Whoops!</b>\nNice try {update?.Message?.From?.FirstName} \uD83D\uDE02. This chat bot only accepts written messages instead.",
                     ParseMode.Html,
                     protectContent: true,
                    replyParameters: update?.Message.Id,
                    replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl($"View Administrator's Profile", "https://linkedin.com/in/helixwchipofya")));

                }
            break;
              
            
        }
    }


    public async Task OnMessage(Message msg, UpdateType type)
    {

        if(msg.Text?.Trim() != string.Empty && msg.Text is not null)
        {
            await Bot.SendChatAction(msg.Chat.Id, ChatAction.Typing);
            var message = await aIChatClient.SendRequest(msg.Text.Trim());
            await Bot.SendMessage(msg.Chat.Id, message);
        }
    }  
}
