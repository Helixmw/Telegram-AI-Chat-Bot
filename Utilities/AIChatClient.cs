using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MWBotApp.Exceptions;
using OpenAI.Chat;
using Telegram.Bot.Types;

namespace MWBotApp.Utilities;
public class AIChatClient : AIClient
{

    public AIChatClient(string gptversion):base(gptversion)
    {
        base.SetChatClient();
        base.SetAudioClient();
    }

    //Sends message to the bot and gets response
    public async Task<string> SendRequest(string message)
    {
        try
        {
            if (message.Trim() == "/start")
            {
                var msg = await greetingMessage();
                return msg;
            }
            else
            {
                var messageText = string.Empty;
                await Task.Run(async () =>
                {
                    //Adds User's message
                    messages.Add(new UserChatMessage(message));
                    /*--Start processing response--*/
                    completionUpdates = chatClient.CompleteChatStreamingAsync(messages);

                    await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                    {
                        if (completionUpdate.ContentUpdate.Count > 0)
                        {
                            messageText += completionUpdate.ContentUpdate[0].Text;
                        }
                    }
                    //Add sent message by assistant to messages
                    messages.Add(new AssistantChatMessage(messageText));
                });

                return messageText;
            }
        }
        catch (Exception)
        {
            throw new TelegramBotException(ErrorMessage);
        }
    }

    //Sends voice note and gets response
    public async Task<string> SendVoiceRequest(byte[] file)
    {
        try
        {
            var messageText = string.Empty;
            if (file is not null)
            {
                if (audioClient is not null)
                {
                    var byteArrayContent = new ByteArrayContent(file);
                    byteArrayContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("audio/mp3");
                    var transcription = await audioClient.TranscribeAudioAsync(byteArrayContent?.Headers?.ContentLocation?.OriginalString);
                    messages.Add(new UserChatMessage(transcription.Value.Text));
                    if (chatClient is not null)
                    {
                        completionUpdates = chatClient.CompleteChatStreamingAsync(messages);
                        await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                        {
                            if (completionUpdate.ContentUpdate.Count > 0)
                            {
                                messageText += completionUpdate.ContentUpdate[0].Text;
                            }
                        }
                    }
                }
            }
            return messageText;
        }
        catch (Exception)
        {
            throw new TelegramBotException(ErrorMessage);
        }
    }

    public async Task<string> greetingMessage()
    {
        try
        {
            var messageText = string.Empty;
            await Task.Run(async () =>
            {
                //Add system prompt message
                messages.Add(new SystemChatMessage(systemPromptMessage));
                /*--Start processing response--*/
                completionUpdates = chatClient.CompleteChatStreamingAsync(messages);
                await foreach (StreamingChatCompletionUpdate completionUpdate in completionUpdates)
                {
                    if (completionUpdate.ContentUpdate.Count > 0)
                    {
                        messageText += completionUpdate.ContentUpdate[0].Text;
                    }
                }
                //Add sent message by assistant to messages
                messages.Add(new AssistantChatMessage(messageText));
            });
            return messageText;
        }
        catch (Exception)
        {
            throw new TelegramBotException(ErrorMessage);
        }
    }
}
