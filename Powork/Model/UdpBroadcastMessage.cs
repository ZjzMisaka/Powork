using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model
{
    public class UdpBroadcastMessage
    {
        public IPEndPoint IPEndPoint { get; set; }
        public Guid UniqueID { get; set; }
    }
}
