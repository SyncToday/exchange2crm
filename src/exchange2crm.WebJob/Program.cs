using Orleans;
using Orleans.Runtime.Host;
using System.Net;
using Orleans.Runtime.Configuration;
using exchange2crm.Interfaces;
namespace exchange2crm.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    internal static class Program
    {
        private static SiloHost siloHost;

        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        private static void Main()
        {
            Common.initConsoleLog();

            var appConfig =
                typeof(Program).Assembly.GetName().Name + ".exe.config";

            var hostDomain = AppDomain.CreateDomain("OrleansHost", null,
                new AppDomainSetup()
                {
                    AppDomainInitializer = InitSilo,
                    ConfigurationFile = appConfig
                });


            var clientConfig = new ClientConfiguration();
            clientConfig.Gateways.Add(
                new IPEndPoint(IPAddress.Loopback, 30000)
            );

            GrainClient.Initialize(clientConfig);

            var importExchange =
                GrainClient.GrainFactory.GetGrain<IImportExchangeContacts>(0L);

            importExchange.Import().Wait();

            Console.WriteLine("Orleans Silo is running.");
            Console.WriteLine("Press any key to terminate...");
            Console.ReadKey(intercept: true);

            hostDomain.DoCallBack(ShutdownSilo);
        }

        private static void InitSilo(String[] args)
        {
            Common.initConsoleLog();

            siloHost = new SiloHost(Dns.GetHostName())
            {
                ConfigFileName = "OrleansConfiguration.xml"
            };

            siloHost.InitializeOrleansSilo();
            if (!siloHost.StartOrleansSilo())
            {
                throw new Exception(
                    $"Failed to start Orleans silo '{siloHost.Name}' " +
                    $"as a {siloHost.Type} node"
                );
            }
        }

        private static void ShutdownSilo()
        {
            if (siloHost != null)
            {
                siloHost.Dispose();
                siloHost = null;
            }
        }
    }
}
