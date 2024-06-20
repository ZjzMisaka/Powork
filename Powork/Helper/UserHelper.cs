using Powork.Model;
using Powork.Repository;

namespace Powork.Helper
{
    public static class UserHelper
    {
        public static bool IsUserLogon()
        {
            return GlobalVariables.SelfInfo != null;
        }
    }
}
