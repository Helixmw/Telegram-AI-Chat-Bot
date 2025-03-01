using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWBotApp.Utilities;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MWBotApp.Models
{
    public class MessageResponse
    {
        public long ChatId
        {
            get; set;
        }

        public AIChatClient AIChatClient
        {
            get;set;
        }

        public string Message
        {
            get;set;
        }

        public TelegramBotClient BotClient
        {
            get;set;
        }
    }
}
