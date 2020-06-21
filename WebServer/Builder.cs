using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace WebServer
{
    public static class Builder
    {
        public static IWebHost CreateWebHostBuilder() =>
        WebHost.CreateDefaultBuilder(new string[0])
            .UseStartup<Startup>()
            .ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning))
            .UseUrls("http://0.0.0.0:5000/")
            .Build();
    }
}
