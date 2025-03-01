using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MWBotApp.Models
{
    class MessageResponseParameters
    {
        public TelegramBotClient BotClient
        {
            get; set;
        } = null!;

        public Update? Update
        {
            get;set;
        }

        public Exception? Exception
        {
            get;set;
        }
            

        public CancellationToken CancellationToken
        {
            get;set;
        }

       
    }
}
