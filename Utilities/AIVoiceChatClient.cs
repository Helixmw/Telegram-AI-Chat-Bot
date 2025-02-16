using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
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

    //Sending voice request using voice note
    public async Task SendVoiceRequest(TGFile file, ITelegramBotClient botClient)
    {
       
            if (file.FilePath is not null) {

                if (audioClient is not null)
                {
                    var transcription = await audioClient.TranscribeAudioAsync(file.FilePath);
                    messages.Add(new UserChatMessage(transcription.Value.Text));
                }
            }
           
     }

    
}
