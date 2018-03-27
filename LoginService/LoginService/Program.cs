using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LoginService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string urls = args.Length > 0 ? args[0] : "http://localhost:4201/";

            BuildWebHost(urls).Run();
        }

        public static IWebHost BuildWebHost(string urls) =>
            WebHost.CreateDefaultBuilder()
                .UseStartup<Startup>()
                .UseUrls(urls)
                .Build();
    }
}
