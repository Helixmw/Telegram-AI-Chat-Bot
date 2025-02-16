using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MWBotApp.Utilities;
public static class SettingsConfiguration
{
    public static void Configure()
    {
        var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfiguration configuration = builder.Build();
    }
}
