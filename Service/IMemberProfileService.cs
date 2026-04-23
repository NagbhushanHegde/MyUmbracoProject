namespace MyUmbracoProject.Services
{
    public interface IMemberProfileService
    {
        Task<MemberProfileData?> GetProfileAsync(string email);
        Task<ServiceResult> UpdateProfileAsync(    
            string email, string name, string city);
    }

    public class MemberProfileData
    {
        public string Name  { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string City  { get; set; } = string.Empty;
    }
}