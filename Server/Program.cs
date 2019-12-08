using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Domain;

namespace Server
{
    class Program
    {
        // Порт, прослушиваемый сервером
        private const int port = 8888;
        //размер буфера для сообщений
        private const int bufferSize = 2048;
        // Команда завершения работы сервера
        private const string stopCommand = "Y";
        // Команда вывода списка полученных сообщений и БД
        private const string printCommand = "print";
        /// <summary>
        /// Основная программа приложения сервера
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            var server = new TcpServer(IPAddress.Any, port);
            server.OnDataReceived +=  (sender, e) =>
            {
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                string message = string.Empty;
                do
                {
                    bytesRead = e.Stream.Read(buffer, 0, bufferSize);
                    message += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
                while (bytesRead > 0 && e.Stream.DataAvailable);

                var serverMessage = new ServerMessage
                {
                    Date = e.Date,
                    Text = message,
                    Ip = e.Ip
                };
                // Save received message to the db
                SaveMessage(serverMessage);
            };
            var serverTask = server.StartAsync();
            string messageString = string.Empty;
            while (messageString.ToUpper() != stopCommand)
            {
                Console.WriteLine("Enter command: ");
                messageString = Console.ReadLine();
                if (messageString.ToUpper() == stopCommand)
                    server.Stop();
                else if (messageString == printCommand)
                    PrintResults();
            }
            await serverTask;
        }   
        /// <summary>
        /// Вывод сообщений из БД
        /// </summary>
        public static void PrintResults()
        {
            using (var dbContext = new ClientServerDbContext())
            {
                var messages = dbContext.ServerMessages.OrderByDescending(m => m.Date);
                Console.WriteLine("{0,-40} | {1,-15} | {2,-20}", "Message:", "IP address:", "Date:");
                Console.WriteLine();
                foreach (var sm in messages)
                {
                    Console.WriteLine("{0,-40} | {1,-15} | {2,-20}",
                        sm.Text, sm.Ip, sm.Date.ToString("dd.mm.yyyy hh:mm::ss"));
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Сохранение полученного сообщения в БД
        /// </summary>
        /// <param name="serverMessage">Сообщение, полученное от клиента</param>
        private static void SaveMessage(Domain.ServerMessage serverMessage)
        {
            using (var dbContext = new Domain.ClientServerDbContext())
            {
                var m = dbContext.Add(serverMessage);
                dbContext.SaveChanges();
            }
        }
    }
}
