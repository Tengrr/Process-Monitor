namespace ProcessMonitor.WebApi
{
    using Azure.Messaging.ServiceBus;
    using ProcessMonitor.WebApi.Models;

    public class MessageSender
    {
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://lucifer.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=MBvPBQvD7F2QfKNL2UvyAuN+45NPzj8q6KOsPvBoHOI=";

        // name of your Service Bus queue
        static string queueName = "queue1";

        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client;

        // the sender used to publish messages to the queue
        static ServiceBusSender sender;

        public static async Task SendOperationToQueue(Operation operation)
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.
            //
            // Create the clients that we'll use for sending and processing messages.
            client = new ServiceBusClient(connectionString);
            sender = client.CreateSender(queueName);

            ServiceBusMessage message = new ServiceBusMessage();

            message.ApplicationProperties["Name"] = operation.name;
            message.ApplicationProperties["Id"] = operation.id;
            message.ApplicationProperties["Pid"] = operation.pid;

            await sender.SendMessageAsync(message);
            Console.WriteLine("Successfully sent");

            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
