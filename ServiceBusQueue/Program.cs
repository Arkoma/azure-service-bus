using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceBusQueue
{
    class Program
    {
        // Add the details for the connection string and queue name
        const string connString = "Endpoint=sb://demoservice2020.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=+j2ZHU5JeWNeOu7yar0BCiKIi+BZu+7IlV+moCcSwA8=";
        const string queue_name = "demoqueue";
        static QueueClient l_queueClient;
        static void Main(string[] args)
        {
            MainFunction().GetAwaiter().GetResult();

        }
        static async Task MainFunction()
        {
            l_queueClient = new QueueClient(connString, queue_name);
            //SendMessage().Wait();
            ReceiveMessage().Wait();
            Console.ReadKey();
            await l_queueClient.CloseAsync();
        }

        static async Task SendMessage()
        {
            // Construct and encode the message
            string l_messageBody = "This is a sample message";
            var l_message = new Message(Encoding.UTF8.GetBytes(l_messageBody));

            Console.WriteLine("Sending the message");

            // Let the queue client send the message
            await l_queueClient.SendAsync(l_message);
            await l_queueClient.CloseAsync();
        }

        static async Task ReceiveMessage()
        {

            Console.WriteLine("Receiving the message");
            var l_Options = new MessageHandlerOptions(ExceptionHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            // We have to register a function that will be used to receive the messages
            l_queueClient.RegisterMessageHandler(MessageProcessor, l_Options);
         
        }
        static Task ExceptionHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            // This function will be called if there are any exceptions when receiving the messsage
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine(context.Action);
            return Task.CompletedTask;
        }

        static async Task MessageProcessor(Message message, CancellationToken token)
        {
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            
            // Complete the receival of the message so that it is not read by anyone else
            await l_queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }
    }
}
