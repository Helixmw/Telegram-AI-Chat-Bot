using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MWBotApp.Models;
using MWBotApp.Utilities;
using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MWBotApp.Logic;
public abstract class BaseProcessor
{
    protected readonly IConfiguration Configuration;

    protected BaseProcessor()
    {
        Configuration = new ConfigurationBuilder().AddUserSecrets<BaseProcessor>().Build();
    }

    protected async Task ProcessMessageResponse(MessageResponse response)
    {
        await response.BotClient.SendChatAction(response.ChatId, ChatAction.Typing);
        var message = await response.AIChatClient.SendRequest(response.Message);
        await response.BotClient.SendMessage(response.ChatId, message);
    }

    

}
