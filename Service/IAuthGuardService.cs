namespace MyUmbracoProject.Services
{
    public interface IAuthGuardService
    {
        bool IsAuthenticated(System.Security.Principal.IPrincipal user);
        string GetCurrentUserEmail(System.Security.Principal.IPrincipal user);
    }
}