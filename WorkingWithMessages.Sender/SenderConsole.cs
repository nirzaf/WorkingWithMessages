using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkingWithMessages.Config;
using WorkingWithMessages.MessageEntities;

namespace WorkingWithMessages.Sender
{
    class SenderConsole
    {
        static async Task Main(string[] args)
        {
            WriteLine("Sender Console - Hit enter", ConsoleColor.White);
            Console.ReadLine();

            //ToDo: Comment in the appropriate method

            //await SendTextString("The quick brown fox jumps over the lazy dog");

            //await SendPizzaOrderAsync();

            //await SendControlMessageAsync();

            //await SendPizzaOrderListAsMessagesAsync();
        
            //await SendPizzaOrderListAsBatchAsync();

            //await SendTextStringAsMessagesAsync("The quick brown fox jumps over the lazy dog");

            //await SendTextStringAsBatchAsync("The quick brown fox jumps over the lazy dog");



            WriteLine("Sender Console - Complete", ConsoleColor.White);
            Console.ReadLine();
        }

        static async Task SendTextString(string text)
        {
            WriteLine("SendTextStringAsMessagesAsync", ConsoleColor.Cyan);

            // Create a client
            var client = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            Write("Sending...", ConsoleColor.Green);

            var message = new Message(Encoding.UTF8.GetBytes(text));
            await client.SendAsync(message);

            WriteLine("Done!", ConsoleColor.Green);


            Console.WriteLine();

            // Always close the client
            await client.CloseAsync();
        }


        static async Task SendTextStringAsMessagesAsync(string text)
        {
            WriteLine("SendTextStringAsMessagesAsync", ConsoleColor.Cyan);

            // Create a client
            var client = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            Write("Sending:", ConsoleColor.Green);


            foreach (var letter in text.ToCharArray())
            {
                // Create an empty message and set the label.
                var message = new Message();
                message.Label = letter.ToString();


                // Send the message
                await client.SendAsync(message);
                Write(message.Label, ConsoleColor.Green);
            }


            Console.WriteLine();
            Console.WriteLine();

            // Always close the client
            await client.CloseAsync();
        }


        static async Task SendTextStringAsBatchAsync(string text)
        {
            WriteLine("SendTextStringAsBatchAsync", ConsoleColor.Cyan);

            // Create a client
            var client = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            Write("Sending:", ConsoleColor.Green);
            var taskList = new List<Task>();

            var messageList = new List<Message>();

            foreach (var letter in text.ToCharArray())
            {
                // Create an empty message and set the label.
                var message = new Message();
                message.Label = letter.ToString();

                messageList.Add(message);

            }

            await client.SendAsync(messageList);


            Console.WriteLine();
            Console.WriteLine();

            // Always close the client
            await client.CloseAsync();
        }




        static async Task SendControlMessageAsync()
        {
            WriteLine("SendControlMessageAsync", ConsoleColor.Cyan);

            // Create a message with no body.
            var message = new Message()
            {
                Label = "Control"
            };

            // Add some properties to the property collection
            message.UserProperties.Add("SystemId", 1462);
            message.UserProperties.Add("Command", "Pending Restart");
            message.UserProperties.Add("ActionTime", DateTime.UtcNow.AddHours(2));

            // Send the message
            var client = new QueueClient(Settings.ConnectionString, Settings.QueueName);
            Write("Sending control message...", ConsoleColor.Green);
            await client.SendAsync(message);
            WriteLine("Done!", ConsoleColor.Green);
            Console.WriteLine();
            await client.CloseAsync();

        }

        static async Task SendPizzaOrderAsync()
        {
            WriteLine("SendPizzaOrderAsync", ConsoleColor.Cyan);

            var order = new PizzaOrder()
            {
                CustomerName = "Alan Smith",
                Type = "Hawaiian",
                Size = "Large"
            };

            // Serialize the order object
            var jsonPizzaOrder = JsonConvert.SerializeObject(order);

            // Create a Message
            var message = new Message(Encoding.UTF8.GetBytes(jsonPizzaOrder))
            {
                Label = "PizzaOrder",
                ContentType = "application/json"
            };


            // Send the message...
            var client = new QueueClient(Settings.ConnectionString, Settings.QueueName);
            Write("Sending order...", ConsoleColor.Green);
            await client.SendAsync(message);
            WriteLine("Done!", ConsoleColor.Green);
            Console.WriteLine();
            await client.CloseAsync();

        }

        static async Task SendPizzaOrderListAsMessagesAsync()
        {
            WriteLine("SendPizzaOrderListAsMessagesAsync", ConsoleColor.Cyan);

            var pizzaOrderList = GetPizzaOrderList();

            // Create a queue client
            var client = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            WriteLine("Sending...", ConsoleColor.Yellow);
            var watch = Stopwatch.StartNew();

            foreach (var pizzaOrder in pizzaOrderList)
            {
                var jsonPizzaOrder = JsonConvert.SerializeObject(pizzaOrder);
                var message = new Message(Encoding.UTF8.GetBytes(jsonPizzaOrder))
                {
                    Label = "PizzaOrder",
                    ContentType = "application/json"
                };
                await client.SendAsync(message);
            }

            WriteLine($"Sent { pizzaOrderList.Count } orders! - Time: { watch.ElapsedMilliseconds } milliseconds, that's { pizzaOrderList.Count / watch.Elapsed.TotalSeconds } messages per second.", ConsoleColor.Green);
            Console.WriteLine();
            Console.WriteLine();
        }


        static async Task SendPizzaOrderListAsBatchAsync()
        {
            WriteLine("SendPizzaOrderListAsBatchAsync", ConsoleColor.Cyan);

            var pizzaOrderList = GetPizzaOrderList();
            var client = new QueueClient(Settings.ConnectionString, Settings.QueueName);

            var watch = Stopwatch.StartNew();
            var messageList = new List<Message>();

            foreach (var pizzaOrder in pizzaOrderList)
            {
                var jsonPizzaOrder = JsonConvert.SerializeObject(pizzaOrder);
                var message = new Message(Encoding.UTF8.GetBytes(jsonPizzaOrder))
                {
                    Label = "PizzaOrder",
                    ContentType = "application/json"
                };
                messageList.Add(message);
            }


            WriteLine("Sending...", ConsoleColor.Yellow);
            await client.SendAsync(messageList);

            // Always close the client!
            await client.CloseAsync();



            WriteLine($"Sent { pizzaOrderList.Count } orders! - Time: { watch.ElapsedMilliseconds } milliseconds, that's { pizzaOrderList.Count / watch.Elapsed.TotalSeconds } messages per second.", ConsoleColor.Green);
            Console.WriteLine();
            Console.WriteLine();

        }

        static List<PizzaOrder> GetPizzaOrderList()
        {
            // Create some data
            string[] names = { "Alan", "Jennifer", "James" };
            string[] pizzas = { "Hawaiian", "Vegetarian", "Capricciosa", "Napolitana" };

            var pizzaOrderList = new List<PizzaOrder>();
            for (int pizza = 0; pizza < pizzas.Length; pizza++)
            {
                for (int name = 0; name < names.Length; name++)
                {

                    PizzaOrder order = new PizzaOrder()
                    {
                        CustomerName = names[name],
                        Type = pizzas[pizza],
                        Size = "Large"
                    };
                    pizzaOrderList.Add(order);
                }
            }
            return pizzaOrderList;
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
