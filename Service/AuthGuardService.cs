using System.Security.Principal;

namespace MyUmbracoProject.Services
{
    public class AuthGuardService : IAuthGuardService
    {
        public bool IsAuthenticated(IPrincipal user) =>
            user.Identity != null && user.Identity.IsAuthenticated;

        public string GetCurrentUserEmail(IPrincipal user) =>
            user.Identity?.Name ?? string.Empty;
    }
}