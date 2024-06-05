using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powork.Model
{
    public class Team
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public List<User> MemberList { get; set; }
    }
}
