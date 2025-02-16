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
            .WithParameter(
        (pi, ctx) => pi.ParameterType == typeof(string),
        (pi, ctx) => configuration.GetSection("ApiKeys:TelegramKey").Value)
        .WithParameter(
        (pi, ctx) => pi.ParameterType == typeof(CancellationToken),
        (pi, ctx) => CancellationToken.None);
        builder.RegisterType<AIChatClient>().WithParameter(
            (pi, ctx) => pi.ParameterType == typeof(string),
            (pi, ctx) => configuration.GetSection("GptVersions:Chat").Value);
        builder.RegisterType<AIVoiceChatClient>().WithParameter(
            (pi, ctx) => pi.ParameterType == typeof(string),
            (pi, ctx) => configuration.GetSection("GptVersions:Voice").Value
            );
        builder.RegisterType<TelegramBotProcessor>();
        return builder.Build();
    }

   
}
