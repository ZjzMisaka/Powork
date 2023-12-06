using PowerThreadPool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork
{
    static public class GlobalVariables
    {
        static public Guid UniqueID { get; } = Guid.NewGuid();
        static public int UdpPort { get; set; } = 1096;
        static public int TcpPort { get; set; } = 614;
    }
}
