using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using COD.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace COD
{
    public class Program
    {
        public static readonly Bot bot = new Bot();
        public static void Main(string[] args)
        {
            //read DB
            Thread DBStream = new Thread(ReadDB.ThreadDB);
            DBStream.Start();
            ////Bot Configurations
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            bot.GetBotClient();
            //run web
            CreateHostBuilder(args).Build().Run();
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
