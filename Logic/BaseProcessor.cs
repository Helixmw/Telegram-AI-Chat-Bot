using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MWBotApp.Logic;
public abstract class BaseProcessor
{
    protected readonly IConfiguration Configuration;

    protected BaseProcessor()
    {
        Configuration = new ConfigurationBuilder().AddUserSecrets<BaseProcessor>().Build();
    }

}
