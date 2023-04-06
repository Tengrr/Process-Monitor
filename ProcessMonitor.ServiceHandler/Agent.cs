using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ProcessInfo
{   
    public int pid;
    public string processName;
    public int sessionId;   
}

namespace ProcessMonitor.ServiceHandler
{


    public class Agent
    {
        // connection string to your Service Bus namespace
        static string connectionString = "Endpoint=sb://lucifer.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=MBvPBQvD7F2QfKNL2UvyAuN+45NPzj8q6KOsPvBoHOI=";

        // name of your Service Bus queue
        static string queue1Name = "queue1";
        static string queue2Name = "queue2";


        // the client that owns the connection and can be used to create senders and receivers
        static ServiceBusClient client = new ServiceBusClient(connectionString);

        // the processor that reads and processes messages from the queue
        static ServiceBusProcessor processor;

        // the sender used to publish messages to the queue
        static ServiceBusSender sender;

        static async Task ProcessListSender(string id)
        {
            // The Service Bus client types are safe to cache and use as a singleton for the lifetime
            // of the application, which is best practice when messages are being published or read
            // regularly.

            //create the sender
            sender = client.CreateSender(queue2Name);

            //将进程信息封装到processInfo对象中，建立一个processInfoList存放这些对象
            var processList = Process.GetProcesses().ToList();
            List<ProcessInfo> processInfoList = new List<ProcessInfo>();
            ProcessInfo processInfo;
            foreach (var process in processList)
            {
                processInfo = new ProcessInfo();
                processInfo.pid = process.Id;
                processInfo.processName = process.ProcessName;
                processInfo.sessionId = process.SessionId;
                processInfoList.Add(processInfo);
            }

            //获得processInfoList的Json形式
            JsonSerializer serializer = new JsonSerializer();
            StringWriter sw = new StringWriter();
            serializer.Serialize(new JsonTextWriter(sw), processInfoList);
            
            //将operationid加在result前面，用'^'分隔、、、、、、、、、、、、、、、、、、、、、、、、、、、、、、、
            string result = id + "^" + sw.GetStringBuilder().ToString();

            //将进程列表写入消息
            ServiceBusMessage message = new ServiceBusMessage(result);
            //message.ApplicationProperties["Id"] = id;
            //message.ApplicationProperties["ProcessList"] = sw.GetStringBuilder().ToString();

            // send the message
            await sender.SendMessageAsync(message);
            Console.WriteLine("Successfully sent");

            await sender.DisposeAsync();

        }

        // handle received messages
        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            //查看消息中是否包含属性Name
            if (args.Message.ApplicationProperties.ContainsKey("Name"))
            {
                string name = args.Message.ApplicationProperties["Name"].ToString();
                string id = args.Message.ApplicationProperties["Id"].ToString();
                Console.WriteLine($"Name: {name}");

                //处理Name="ProcessList"的消息
                if (name.Equals("ProcessList"))
                {
                    //发送进程列表至另一队列
                    await ProcessListSender(id);
                    Console.WriteLine("1");
                }
                //处理Name="KillProcess"的消息
                else if (name.Equals("KillProcess"))
                {
                    //获取进程pid
                    int pid = int.Parse(args.Message.ApplicationProperties["Pid"].ToString());
                    Console.WriteLine($"Pid: {pid}");

                    //通过pid杀死进程
                    Process killedprocess = Process.GetProcessById(pid);
                    killedprocess.Kill();
                    Console.WriteLine($"killed the process where pid={pid}");
                    Thread.Sleep(500);

                    //发送进程列表至另一队列
                    await ProcessListSender(id);
                    Console.WriteLine("2");
                }
                else
                {
                }
            }
            else
            {
            }

            // complete the message. messages are eliminated from the queue. 
            await args.CompleteMessageAsync(args.Message);
        }

        // handle any errors when receiving messages
        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        public static async Task StartProcessing()
        {

            // create a processor that we can use to process the messages
            processor = client.CreateProcessor(queue1Name, new ServiceBusProcessorOptions());

            // add handler to process messages
            processor.ProcessMessageAsync += MessageHandler;

            // add handler to process any errors
            processor.ProcessErrorAsync += ErrorHandler;

            // start processing 
            await processor.StartProcessingAsync();
        }

        public static async Task StopProcessing()
        {
            // stop processing 
            Console.WriteLine("\nStopping the receiver...");
            await processor.StopProcessingAsync();
            Console.WriteLine("Stopped receiving messages");


            // Calling DisposeAsync on client types is required to ensure that network
            // resources and other unmanaged objects are properly cleaned up.
            await processor.DisposeAsync();
            await client.DisposeAsync();
        }
    }
}
