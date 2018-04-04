using System;

namespace LoginService
{
    
    internal class Settings
    {
        public static string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString"); // , EnvironmentVariableTarget.Process);
        public static string HCConnectionString = "Server=tcp:microservice2.database.windows.net,1433;Initial Catalog = testSQL2; Persist Security Info=False;User ID = azureuser; Password=Pa$$w0rd1234;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout = 30;";
        public static string ApplicationInsightsKey = Environment.GetEnvironmentVariable("InstrumentationKey");
        public static string testConfigMap = Environment.GetEnvironmentVariable("testdata");
        public static string testSecrets = Environment.GetEnvironmentVariable("testsecret");

        public Settings()
        {
        }
    }
}
