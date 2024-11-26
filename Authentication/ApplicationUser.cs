using Microsoft.AspNetCore.Identity;

namespace RoadReady.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        public string Password { get; internal set; }
    }
}
