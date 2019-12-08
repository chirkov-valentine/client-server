using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Client
{
    class Program
    {
        // Порт, по которому подключаемся к серверу
        private const int port = 8888;
        // IP адрес сервера
        private const string server = "127.0.0.1";
        private static ConcurrentQueue<ClientMessage> queue_ = new ConcurrentQueue<ClientMessage>();
        /// <summary>
        /// Основная программа, для выхода ввести "y" или "Y"
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            Guid clientId = Guid.NewGuid();
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;
            string messageString = string.Empty;
            Task task = Task.Run(() =>
            {
                // Загружаем из БД неотправленные сообщения
                LoadMessages();
            
                while (true)
                {
                    // Проверяем завершен ли таск
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    ClientMessage message;
                    if (queue_.TryDequeue(out message))
                    {
                        // Попытка посылки сообщения
                        using (TcpClient client = new TcpClient())
                        {
                            try
                            {
                                client.Connect(server, port);
                                Stream stm = client.GetStream();

                                UTF8Encoding asen = new UTF8Encoding();
                                byte[] ba = asen.GetBytes(message.Message);
                                stm.Write(ba, 0, ba.Length);
                                // Удаляем из БД, если успешно отправили
                                DeleteMessageFromDb(message.ClientMessageID);
                            }
                            catch (Exception ex)
                            {
                                // Вернуть обратно в очередь
                                queue_.Enqueue(message);
                                // Добавляем в БД, если не добавили ранее
                                AddMessageToDb(message);
                            }
                        }
                    }
                    // Подождать до следующей проверки
                    Thread.Sleep(2000);

                }
            });
            while (messageString.ToUpper() != "Y")
            {
                Console.WriteLine("Enter string to send: ");
                messageString = Console.ReadLine();
                if (messageString.ToUpper() == "Y")
                    cancelTokenSource.Cancel();
                else
                {
                    var clientMessage = new ClientMessage
                    {
                        Message = messageString,
                    };
                    queue_.Enqueue(clientMessage);
                }
            }
            await task;
        }

        /// <summary>
        /// Загрузка неотправленных сообщений из БД
        /// </summary>
        private static void LoadMessages()
        {
            using (var dbContext = new ClientServerDbContext())
            {
   
                foreach (var dbMessage in dbContext.ClientMessages)
                {
                    queue_.Enqueue(dbMessage);
                }
            }
        }

        /// <summary>
        /// Добавить сообщение в БД
        /// </summary>
        /// <param name="clientMessage"></param>
        private static void AddMessageToDb(ClientMessage clientMessage)
        {
            // Сообщение уже было добавлено
            if (clientMessage.ClientMessageID > 0)
                return;
            using (var dbContext = new ClientServerDbContext())
            {
                dbContext.Add(clientMessage);
                dbContext.SaveChanges();
            }
        }
        /// <summary>
        /// Удалить сообщение из БД
        /// </summary>
        /// <param name="clientMessageId"></param>
        private static void DeleteMessageFromDb(int clientMessageId)
        {
            using (var dbContext = new ClientServerDbContext())
            {
                dbContext.Database.ExecuteSqlInterpolated($"DELETE FROM ClientMessages WHERE ClientMessageID = {clientMessageId}");
                dbContext.SaveChanges();
            }
        }



    }
}
