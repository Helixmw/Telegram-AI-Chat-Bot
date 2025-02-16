using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MWBotApp.Exceptions;
using OpenAI.Chat;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MWBotApp.Utilities;
public class AIVoiceChatClient : AIClient
{
    public AIVoiceChatClient(string gptversion):base(gptversion)
    {
        base.SetAudioClient();
    }

    //Sending voice note and gets voice response
    public async Task SendVoiceRequest(TGFile file, ITelegramBotClient botClient)
    {
        try
        {
            var script = string.Empty;
            if (file.FilePath is not null)
            {
                if (audioClient is not null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await botClient.DownloadFile(file.FilePath, memoryStream);
                        memoryStream.Position = 0;
                        var transcription = await audioClient.TranscribeAudioAsync(memoryStream, Path.GetFileName(file.FilePath));
                        script = transcription.Value.Text;
                        messages.Add(new UserChatMessage(transcription.Value.Text));
                    }
                }
            }
        }
        catch (Exception)
        {
            throw new TelegramBotException("Sorry but I can't get voice notes yet.");
        }
     }
   
}
