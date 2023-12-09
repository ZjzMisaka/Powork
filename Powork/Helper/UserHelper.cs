using Powork.Model;
using Powork.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Powork.Helper
{
    static internal class UserHelper
    {
        static internal bool IsUserLogon()
        {
            string ip = GlobalVariables.LocalIP.ToString();

            List<User> userList = UserRepository.SelectUserByIp(ip);
            return userList.Count == 1;
        }
    }
}
