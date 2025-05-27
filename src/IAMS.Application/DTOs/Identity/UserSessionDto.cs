using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMS.Application.DTOs.Identity
{
    public class UserSessionDto
    {
        public string UserId { get; set; }
        public string? Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Permissions { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
