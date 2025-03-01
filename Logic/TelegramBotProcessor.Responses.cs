using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MWBotApp.Exceptions;
using MWBotApp.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MWBotApp.Logic
{
    public partial class TelegramBotProcessor : BaseProcessor
    {
        //Starts messages response when a Telegram message is sent
        private async Task StartMessageResponse(MessageResponseParameters parameters)
        {
            switch (parameters?.Update?.Message?.Type)
            {
                case MessageType.Text:
                    chatId = parameters.Update.Message.Chat.Id;
                    if (parameters.Update.Message?.Text?.Trim() != string.Empty && parameters.Update.Message?.Text is not null)
                    {
                        var msg = parameters.Update.Message.Text.Trim();
                        try
                        {
                            var response = new MessageResponse
                            {
                                ChatId = parameters.Update.Message.Chat.Id,
                                AIChatClient = aIChatClient,
                                BotClient = parameters.BotClient,
                                Message = msg
                            };
                            ThreadPool.QueueUserWorkItem(async state => await ProcessMessageResponse(response));
                        }
                        catch (TelegramBotException ex)
                        {
                            await HandleErrorAsync(parameters.BotClient, ex, parameters.CancellationToken);
                        }
                    }
                    break;
                case MessageType.Voice:
                    chatId = parameters.Update.Message.Chat.Id;
                    if (parameters.Update.Message?.Voice is not null)
                    {
                        var fileId = parameters.Update.Message.Voice.FileId;
                        var file = await parameters.BotClient.GetFile(fileId);
                        try
                        {
                            await aIVoiceChatClient.SendVoiceRequest(file, parameters.BotClient);
                        }
                        catch (TelegramBotException ex)
                        {
                            await HandleErrorAsync(parameters.BotClient, ex, parameters.CancellationToken);
                        }
                    }
                    break;
                default:
                    if (parameters?.Update?.Message is not null)
                    {
                        chatId = parameters.Update.Message.Chat.Id;
                        await parameters.BotClient.SendChatAction(parameters.Update.Message.Chat.Id, ChatAction.Typing);
                        var me = await parameters.BotClient.GetMe();
                        var error_message = $"<b>Whoops!</b>\nNice try {parameters.Update?.Message?.From?.FirstName} \uD83D\uDE02. This chat bot only accepts written messages instead.";

                        //Error Message Parameters
                        var botError = new TelegramBotException(
                            error_message,
                            ParseMode.Html,
                            true,
                            parameters.Update?.Message.Id,
                            new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithUrl($"View Administrator's Profile", Configuration.GetSection("Administrator:ProfileUrl")?.Value ?? string.Empty)
                            ));
                        await HandleErrorAsync(parameters.BotClient, botError, parameters.CancellationToken);

                    }
                    break;
            }
        }
        
        //Sends response when bot encounters an error
        private async Task StartErrorMessageResponse(MessageResponseParameters parameters)
        {
            await parameters.BotClient.SendChatAction(chatId, ChatAction.Typing);
            var botException = parameters?.Exception as TelegramBotException;
            if (botException is not null)
                if(parameters?.BotClient is not null)
                await SendErrorMessage(parameters.BotClient, chatId, botException.BotErrorMessage, botException.ParseMode, botException.ProtectContent, botException.ReplyParameters, botException.ReplyMarkup);
            Console.WriteLine($"Your Error: {botException?.Message}");
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
    }
}
