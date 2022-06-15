using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Text;
using System.Threading.Tasks;
using WorkingWithMessages.Config;
using WorkingWithMessages.MessageEntities;
using System.Threading;
using Newtonsoft.Json;
using Microsoft.ServiceBus;

namespace WorkingWithMessages.Receiver
{
    class ReceiverConsole
    {

        private static QueueClient QueueClient;

        static async Task Main(string[] args)
        {

            WriteLine("Receiver Console", ConsoleColor.White);

            await RecreateQueueAsync();


            //Comment in the appropriate method

            //ReceiveAndProcessText(1);

            //ReceiveAndProcessPizzaOrdes(1);
            //ReceiveAndProcessPizzaOrdes(5);
            //ReceiveAndProcessPizzaOrdes(100);

            //ReceiveAndProcessControlMessage(1);

            //ReceiveAndProcessCharacters(1);

            //ReceiveAndProcessCharacters(16);


            //WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
            //Console.ReadLine();
            StopReceivingAsync().Wait();

        }



        static void ReceiveAndProcessText(int threads)
        {
            WriteLine($"ReceiveAndProcessText({ threads })", ConsoleColor.Cyan);
            // Create a new client
            QueueClient = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            // Set the options for the message handler
            var options = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = threads,
                MaxAutoRenewDuration = TimeSpan.FromSeconds(30)
            };

            // Create a message pump using OnMessage
            QueueClient.RegisterMessageHandler(ProcessTextMessageAsync, options);


            WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
            Console.ReadLine();
            StopReceivingAsync().Wait();
        }


        static void ReceiveAndProcessControlMessage(int threads)
        {
            WriteLine($"ReceiveAndProcessPizzaOrdes({ threads })", ConsoleColor.Cyan);
            // Create a new client
            QueueClient = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            // Set the options for the message handler
            var options = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = threads,
                MaxAutoRenewDuration = TimeSpan.FromSeconds(30)
            };

            // Create a message pump using OnMessage
            QueueClient.RegisterMessageHandler(ProcessControlMessageAsync, options);


            WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
            Console.ReadLine();
            StopReceivingAsync().Wait();
        }



        static void ReceiveAndProcessPizzaOrdes(int threads)
        {
            WriteLine($"ReceiveAndProcessPizzaOrdes({ threads })", ConsoleColor.Cyan);
            // Create a new client
            QueueClient = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            // Set the options for the message handler
            var options = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = threads,
                MaxAutoRenewDuration = TimeSpan.FromMinutes(10)
            };

            // Create a message pump using RegisterMessageHandler
            QueueClient.RegisterMessageHandler(ProcessPizzaMessageAsync, options);


            WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
            Console.ReadLine();
            StopReceivingAsync().Wait();
        }

        static async Task ProcessPizzaMessageAsync(Message message, CancellationToken token)
        {

            // Deserialize the message body.
            var messageBodyText = Encoding.UTF8.GetString(message.Body);

            var pizzaOrder = JsonConvert.DeserializeObject<PizzaOrder>(messageBodyText);

            // Process the message
            CookPizza(pizzaOrder);

            // Complete the message
            await QueueClient.CompleteAsync(message.SystemProperties.LockToken);

        }

        static async Task ProcessTextMessageAsync(Message message, CancellationToken token)
        {

            // Deserialize the message body.
            var messageBodyText = Encoding.UTF8.GetString(message.Body);

            WriteLine($"Received: { messageBodyText }", ConsoleColor.Green);

            // Complete the message
            await QueueClient.CompleteAsync(message.SystemProperties.LockToken);

        }

        static async Task ProcessControlMessageAsync(Message message, CancellationToken token)
        {



            WriteLine($"Received: { message.Label }", ConsoleColor.Green);

            WriteLine("User properties...", ConsoleColor.Yellow);
            foreach (var property in message.UserProperties)
            {
                WriteLine($"    { property.Key } - { property.Value }", ConsoleColor.Cyan);
            }

            // Complete the message
            await QueueClient.CompleteAsync(message.SystemProperties.LockToken);

        }


        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            WriteLine(exceptionReceivedEventArgs.Exception.Message, ConsoleColor.Red);
            return Task.CompletedTask;
        }



        static void ReceiveAndProcessCharacters(int threads)
        {
            WriteLine($"ReceiveAndProcessCharacters({ threads })", ConsoleColor.Cyan);
            // Create a queue client
            QueueClient = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            // Set the options for the message handler
            var options = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = threads,
                MaxAutoRenewDuration = TimeSpan.FromSeconds(30)
            };

            // Create a message pump
            QueueClient.RegisterMessageHandler(ProcessCharacterMessageAsync, options);

            WriteLine("Receiving, hit enter to exit", ConsoleColor.White);
            Console.ReadLine();
            StopReceivingAsync().Wait();
        }


        static async Task ProcessCharacterMessageAsync(Message message, CancellationToken token)
        {
            Write(message.Label, ConsoleColor.Green);

            // Complete the message receive operation
            await QueueClient.CompleteAsync(message.SystemProperties.LockToken);
        }



        static async Task StopReceivingAsync()
        {
            // Close the client, which will stop the message pump.
            await QueueClient.CloseAsync();
        }


        static async Task RecreateQueueAsync()
        {
            var manager = new ManagementClient(Settings.ConnectionString);
            if (await manager.QueueExistsAsync(Settings.QueueName))
            {
                WriteLine($"Deleting queue: { Settings.QueueName }...", ConsoleColor.Magenta);
                await manager.DeleteQueueAsync(Settings.QueueName);
                WriteLine("Done!", ConsoleColor.Magenta);
            }

            WriteLine($"Creating queue: { Settings.QueueName }...", ConsoleColor.Magenta);
            await manager.CreateQueueAsync(Settings.QueueName);
            WriteLine("Done!", ConsoleColor.Magenta);


        }

        private static void CookPizza(PizzaOrder order)
        {
            WriteLine($"Cooking {  order.Type } for { order.CustomerName }.", ConsoleColor.Yellow);
            Thread.Sleep(5000);
            WriteLine($"    { order.Type } pizza for {  order.CustomerName } is ready!", ConsoleColor.Green);
        }




        private static void WriteLine(string text, ConsoleColor color)
        {
            var tempColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = tempColor;
        }

        private static void Write(string text, ConsoleColor color)
        {
            var tempColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = tempColor;
        }




    }
}
