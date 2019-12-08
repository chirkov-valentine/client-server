using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// Store messages received by server
    /// </summary>
    public class ServerMessage
    {
        public int ServerMessageID { get; set; }
        public string Text { get; set; }
        public string Ip { get; set; }
        public DateTime Date { get; set; }
    }
}
