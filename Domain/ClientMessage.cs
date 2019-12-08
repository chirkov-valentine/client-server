using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// Класс для хранения неотправленных сообщений в БД
    /// </summary>
    public class ClientMessage
    {
        public int ClientMessageID { get; set; }
        public string Message { get; set; }
    }
}
