using ProcessMonitor.ServiceHandler;
using Newtonsoft.Json;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using ProcessMonitor.ConsoleApp;

//The Service Bus client types are safe to cache and use as a singleton for the lifetime
// of the application, which is best practice when messages are being published or read
// regularly.

await ProcessMonitor.ServiceHandler.Agent.StartProcessing();

Console.WriteLine("Wait for a minute and then press any key to end the processing");
Console.ReadKey();

await ProcessMonitor.ServiceHandler.Agent.StopProcessing();

//,:\"^

//string result = "9yrmq6na,[{\"pid\":0,\".........}]"; 

//string[] myQueueItemList = result.Split('^');
//string id = myQueueItemList[0];
//string processList = myQueueItemList[1];

//await CosmosDatabaseHandler.UpdateOperationItemAsync(id, processList);