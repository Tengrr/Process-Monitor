using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace ProcesssMonitor.FunctionApp
{
    public static class ListenToResultqueue
    {
        [FunctionName("ListenToResultqueue")]
        public static async Task RunAsync([ServiceBusTrigger("queue2", Connection = "ServiceBusNamespace")]string myQueueItem)
        {

            string[] myQueueItemList = myQueueItem.Split('^');
            string id = myQueueItemList[0];
            string processList = myQueueItemList[1];

            await CosmosDatabaseHandler.UpdateOperationItemAsync(id, processList);

            Console.WriteLine("Successfully updated");

        }
    }
}
