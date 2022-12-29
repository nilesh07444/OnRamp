using Domain.Enums;

namespace Ramp.Services.Helpers
{
    public static class UserRoleHelper
    {
        public static UserRole GetUserRole(string roleName)
        {
            switch (roleName)
            {
                case "Admin":
                    return UserRole.Admin;
                case "Reseller":
                    return UserRole.Reseller;
                case "CustomerAdmin":
                    return UserRole.CustomerAdmin;
                case "CustomerUser":
                    return UserRole.CustomerStandardUser;
                default:
                    return UserRole.CustomerStandardUser;
            }
        }
    }
}