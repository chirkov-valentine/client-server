using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    /// <summary>
    /// Store unsend messages
    /// </summary>
    public class ClientMessage
    {
        public int ClientMessageID { get; set; }
        public string Message { get; set; }
    }
}
