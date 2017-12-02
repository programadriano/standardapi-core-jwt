using standard.api.Models;
using System.Collections.Generic;

namespace standard.api.Infra.Security
{
    public class Authentication
    {
        internal static UserPerfil AuthenticateUserLogin(string user, string password)
        {

            if (user == "admin" && password == "102030")
            {
                var roles = new List<string>();
                roles.Add("User");
                return new UserPerfil
                {
                    Name = "Admin",
                    Role = "User",
                    Roles = roles
                };
            }

            return null;
        }


    }
}
