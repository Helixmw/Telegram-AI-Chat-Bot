# Telegram API Chat Bot

This is a Telegram bot created utilizing the Telegram Bot Library in order to communicate with bot users. The bot is integrated with an OpenAI .NET API implementation to create a custom AI Chat Assistant that serves its users.
<br/>
<br/>
The bot begins by registering and resolving all required services needed in the entire application

```
 //Resolving services in container created in the ContainerConfig file
        using(var Container = ContainerConfig.Configure(conf.configuration))
        {     
            var botClient = Container.Resolve<TelegramBotClient>();
            var chatClient = Container.Resolve<AIChatClient>();
            var voiceClient = Container.Resolve<AIVoiceChatClient>();
            TelegramBotProcessor BotProcessor = Container.Resolve<TelegramBotProcessor>(
               new TypedParameter(typeof(TelegramBotClient), botClient),
               new TypedParameter(typeof(AIChatClient), chatClient),
               new TypedParameter(typeof(AIVoiceChatClient), voiceClient));
            var Bot = BotProcessor.Bot;

            //Starts up bot services
            Bot.StartReceiving(BotProcessor.HandleUpdateAsync, BotProcessor.HandleErrorAsync, BotProcessor.ReceiverOptions, BotProcessor.tokenSource.Token);

            Console.ReadLine();
```


When a message is sent it handled by the TelegramProcessor to generate appropriate responses

```
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
```

The message sent by the user is then processed by the AIChatClient service

```
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
```

