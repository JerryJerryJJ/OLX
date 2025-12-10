using Microsoft.AspNetCore.Identity;

namespace OLX.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
        public bool? Banned { get; set; } = false;
    }
}