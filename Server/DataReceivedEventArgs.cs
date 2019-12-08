using System;
using System.Net.Sockets;

namespace Server
{
    /// <summary>
    /// Класс, описывающий аргументы для события получения данных от клиента
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        public NetworkStream Stream { get; private set; }
        public string Ip { get; private set; }
        public DateTime Date { get; private set; }
        public DataReceivedEventArgs(NetworkStream stream, string ip, DateTime date)
        {
            Stream = stream;
            Ip = ip;
            Date = date;
        }
    }
}
