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
    static public class UserHelper
    {
        static public bool IsUserLogon()
        {
            string ip = GlobalVariables.LocalIP.ToString();

            List<User> userList = UserRepository.SelectUserByIp(ip);
            return userList.Count == 1;
        }
    }
}
