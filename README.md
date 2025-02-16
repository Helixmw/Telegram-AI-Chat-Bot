# Telegram AI Chat Bot

## Table of Contents
- [Introduction](#introduction)
- [Service Classes](#service-classes)
- [Usage](#usage)
    - [Entry Point](#entry-point)
    - [Telegram Bot Processor](#telegram-bot-processor)
    - [AI Chat Client](#ai-chat-client)
    - [User Responses](#user-responses)

## Introduction
This is a Telegram bot created utilizing the Telegram Bot Library in order to communicate with Telegram users. The bot is integrated with an OpenAI .NET API implementation to create a custom AI Chat Assistant that serves it's users.

## Service Classes
The bot comprises of an ```AIChatClient``` service which derives from the ```AIClient``` base service, and also has a ```TelegramBotProcessor``` class that invokes methods from the ```AIChatClient```.
An important thing to note is that ```AIVoiceChatClient``` has not been fully implemented and so your contribution is definitely welcome. This documentation will only highlight the ```AIChatClient``` service. 

## Usage
### Entry Point
The bot begins as a console app, and registers and resolves all the required services used in the entire application. The ```Bot.StartReceiving``` method starts the service.

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

### Telegram Bot Processor

The ```TelegramBotProcessor``` class handles the different requests sent in from users and provides AI generated responses when a method from the ```AIChatClient``` is invoked. ```HandleUpdatesAsync``` processes those message requests.

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
### AIChatClient
In the ```AIChatClient``` service, the ```SendRequest``` method is invoked providing a user with a generated response.

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

### User Responses
In the ```TelegramBotProcessor``` class, user responses are generated appropriately depending on the kind of input message a user has provided. 
Since our focus is just on Chat Messages, a chat message response will have an assigned Chat Id and a message. It's also important to note that the next code snippet is simply the default implementation you write when use Telegram Bot traditionally.

```await botClient.SendMessage(update.Message.Chat.Id, message);```

Error messages are handled by the ```HandleErrorAsync``` method and looks something like this.
```
//Handles application errors and gives error output
    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        await botClient.SendChatAction(chatId, ChatAction.Typing);
        var botException = exception as TelegramBotException;
        if(botException is not null)
        await SendErrorMessage(botClient, chatId, botException.BotErrorMessage, botException.ParseMode, botException.ProtectContent, botException.ReplyParameters, botException.ReplyMarkup);
        Console.WriteLine($"Your Error: {exception.Message}");
    }
```


