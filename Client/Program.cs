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
        private const int port = 8888;
        private const string server = "127.0.0.1";
        private static ConcurrentQueue<ClientMessage> queue_ = new ConcurrentQueue<ClientMessage>();
        static async Task Main(string[] args)
        {
            Guid clientId = Guid.NewGuid();
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;
            string messageString = string.Empty;
            Task task = Task.Run(() =>
            {
                // Load message, witch is not send
                LoadMessages();
            
                while (true)
                {
                    // Check if task is canceled
                    if (token.IsCancellationRequested)
                    {
                        // Save all unsended messages to the DB
                        //SaveMassages(clientId);
                        return;
                    }
                    ClientMessage message;
                    if (queue_.TryDequeue(out message))
                    {
                        // Try to send message via TCP
                        using (TcpClient client = new TcpClient())
                        {
                            try
                            {
                                client.Connect(server, port);
                                Stream stm = client.GetStream();

                                UTF8Encoding asen = new UTF8Encoding();
                                byte[] ba = asen.GetBytes(message.Message);
                                stm.Write(ba, 0, ba.Length);
                                DeleteMessageFromDb(message.ClientMessageID);
                            }
                            catch (Exception ex)
                            {
                                // Take the message back to the queue
                                queue_.Enqueue(message);
                                AddMessageToDb(message);
                            }
                        }
                    }
                    // Wait for some time
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

        /*private static void SaveMassages()
        { 
            using (var dbContext = new ClientServerDbContext())
            {
                var messagesToRemove = dbContext.ClientMessages;
                dbContext.ClientMessages.RemoveRange(messagesToRemove);
                foreach(var message in queue_.ToArray())
                {
                    dbContext.Add(message);
                }
                dbContext.SaveChanges();
            }
        }*/

        private static void AddMessageToDb(ClientMessage clientMessage)
        {
            // Message was already added
            if (clientMessage.ClientMessageID > 0)
                return;
            using (var dbContext = new ClientServerDbContext())
            {
                dbContext.Add(clientMessage);
                dbContext.SaveChanges();
            }
        }

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
