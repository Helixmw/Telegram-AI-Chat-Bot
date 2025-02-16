using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MWBotApp.Exceptions;
public class TelegramBotException : Exception
{
    public string BotErrorMessage {
        get; set;
    }

    public ParseMode ParseMode { get; set; }

    public bool ProtectContent { get; set; }

    public ReplyParameters? ReplyParameters { get; set; }

    public IReplyMarkup? ReplyMarkup { get; set; }
    public TelegramBotException(string botErrorMessage = "",
        ParseMode parseMode = Telegram.Bot.Types.Enums.ParseMode.None,
        bool protectContent = false,
        ReplyParameters? replyParameters = null,
        IReplyMarkup? replyMarkup = null):base(botErrorMessage)
    {
       
        BotErrorMessage = botErrorMessage;
        ParseMode = parseMode;
        ProtectContent = protectContent;
        ReplyParameters = replyParameters;
        ReplyMarkup = replyMarkup;


    }
}
