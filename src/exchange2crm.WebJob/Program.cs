using System;
using Orleans;
using Orleans.Runtime.Host;
using System.Net;
using Orleans.Runtime.Configuration;
using exchange2crm.Interfaces;
using exchange2crm.Grains;

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
            var importExchange = new ImportExchangeContacts();
            importExchange.Run();
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
