// See https://aka.ms/new-console-template for more information


using Autofac;
using Autofac.Core;
using Microsoft.Extensions.Configuration;
using MWBotApp.Logic;
using MWBotApp.Utilities;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

public class Program
{


    public static void Main(string[] args)
    {
        //Bringing in app configurations such as API Keys
        var conf = new AppConfigurations();

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
        }
    }
 
   
}

public class AppConfigurations
{
    public readonly IConfiguration configuration;

    public AppConfigurations()
    {
        configuration = new ConfigurationBuilder().AddUserSecrets<AppConfigurations>().Build();
    }
}
