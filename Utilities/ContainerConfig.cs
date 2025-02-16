using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using MWBotApp.Logic;
using Telegram.Bot;


namespace MWBotApp.Utilities;
public static class ContainerConfig
{
    
    private static readonly string TelegramApiKey = "";
    public static IContainer Configure(IConfiguration configuration)
    {
     
        var builder = new ContainerBuilder();
        builder.RegisterType<TelegramBotClient>()
            .WithParameters(new TypedParameter(typeof(string), configuration.GetSection("ApiKeys:TelegramKey").Value),
            new TypedParameter(typeof(CancellationToken), CancellationToken.None));
        builder.RegisterType<AIChatClient>().WithParameter(new TypedParameter(typeof(string), configuration.GetSection("GptVersions:gpt-4o-mini").Value));
        builder.RegisterType<AIVoiceChatClient>().WithParameter(new TypedParameter(typeof(string), configuration.GetSection("GptVersions:gpt-4o-mini").Value));
        builder.RegisterType<TelegramBotProcessor>();
        return builder.Build();
    }

   
}
