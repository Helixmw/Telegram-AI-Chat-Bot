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
