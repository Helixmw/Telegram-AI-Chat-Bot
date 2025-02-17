﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace MWBotApp.Logic;
public interface ITelegramBotProcessor
{
    void HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken);

    void HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken);

}
