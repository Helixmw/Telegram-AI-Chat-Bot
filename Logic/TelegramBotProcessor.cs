using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Protocol;
using MWBotApp.Exceptions;
using MWBotApp.Utilities;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;



namespace MWBotApp.Logic;
public class TelegramBotProcessor : BaseProcessor
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

    //Returns error text messages to user when something goes wrong
    private async Task SendErrorMessage(ITelegramBotClient botClient, long chatId, string message, ParseMode parseMode = ParseMode.None, bool protectContent = false, ReplyParameters? replyParameters = null, IReplyMarkup? replyMarkup = null)
    {
        await botClient.SendMessage(chatId,
                    message,
                    parseMode,
                    replyParameters,
                    replyMarkup);
    }

    //Handles application errors and gives error output
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        await botClient.SendChatAction(chatId, ChatAction.Typing);
        var botException = exception as TelegramBotException;
        if(botException is not null)
        await SendErrorMessage(botClient, chatId, botException.BotErrorMessage, botException.ParseMode, botException.ProtectContent, botException.ReplyParameters, botException.ReplyMarkup);
        Console.WriteLine($"Your Error: {exception.Message}");
    }

    //Handles message updates from the user and responds
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        switch (update.Message?.Type)
        {
            case MessageType.Text:
                chatId = update.Message.Chat.Id;
                if (update.Message?.Text?.Trim() != string.Empty && update.Message?.Text is not null)
                {
                    var msg = update.Message.Text.Trim();
                    try
                    {
                        await botClient.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                        var message = await aIChatClient.SendRequest(msg);
                        await botClient.SendMessage(update.Message.Chat.Id, message);
                    }
                    catch (TelegramBotException ex)
                    {
                        await HandleErrorAsync(botClient, ex, cancellationToken);
                    }
                }
            break;
            case MessageType.Voice:
                chatId = update.Message.Chat.Id;
                if(update.Message?.Voice is not null)
                {
                    var fileId = update.Message.Voice.FileId;
                    var file = await botClient.GetFile(fileId);
                    try
                    {
                        await aIVoiceChatClient.SendVoiceRequest(file, botClient);
                    }
                    catch (TelegramBotException ex)
                    {
                        await HandleErrorAsync(botClient, ex, cancellationToken);
                    }
                }
            break;
            default:
                if(update.Message is not null)
                {
                    chatId = update.Message.Chat.Id;
                    await botClient.SendChatAction(update.Message.Chat.Id, ChatAction.Typing);
                    var me = await botClient.GetMe();
                    var error_message = $"<b>Whoops!</b>\nNice try {update?.Message?.From?.FirstName} \uD83D\uDE02. This chat bot only accepts written messages instead.";
                    
                    //Error Message Parameters
                    var botError = new TelegramBotException(
                        error_message,
                        ParseMode.Html,
                        true,
                        update?.Message.Id,
                        new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithUrl($"View Administrator's Profile", Configuration.GetSection("Administrator:ProfileUrl")?.Value ?? string.Empty)
                        ));
                        await HandleErrorAsync(botClient, botError, cancellationToken);
                 
                }
            break;     
        }
    }

  
}
