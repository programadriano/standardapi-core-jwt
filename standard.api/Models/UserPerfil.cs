using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace standard.api.Models
{
    public class UserPerfil
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public IList<string> Roles { get; set; }
    }
}
