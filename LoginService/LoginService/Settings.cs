using System;

namespace LoginService
{
    
    internal class Settings
    {
        public static string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString"); // , EnvironmentVariableTarget.Process);
        public static string ApplicationInsightsKey = Environment.GetEnvironmentVariable("InstrumentationKey");

        public Settings()
        {
        }
    }
}
