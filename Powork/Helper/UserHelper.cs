using Powork.Model;
using Powork.Repository;

namespace Powork.Helper
{
    public static class UserHelper
    {
        public static bool IsUserLogon()
        {
            string ip = GlobalVariables.LocalIP.ToString();

            List<User> userList = UserRepository.SelectUserByIp(ip);
            return userList.Count == 1;
        }
    }
}
