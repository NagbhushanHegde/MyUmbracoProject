using Umbraco.Cms.Core.Services;

namespace MyUmbracoProject.Services
{
    public class MemberProfileService : IMemberProfileService
    {
        private readonly IMemberService _memberService;

        public MemberProfileService(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public Task<MemberProfileData?> GetProfileAsync(string email)
        {
            var member = _memberService.GetByEmail(email);
            if (member == null)
                return Task.FromResult<MemberProfileData?>(null);

            return Task.FromResult<MemberProfileData?>(new MemberProfileData
            {
                Name  = member.Name ?? string.Empty,
                Email = member.Email,
                City  = member.GetValue<string>("city") ?? string.Empty
            });
        }

        public Task<ServiceResult> UpdateProfileAsync(
            string email, string name, string city)
        {
            var member = _memberService.GetByEmail(email);
            if (member == null)
                return Task.FromResult(ServiceResult.Fail("Member not found"));

            member.Name = name;

            if (member.HasProperty("city"))
                member.SetValue("city", city);

            _memberService.Save(member);
            return Task.FromResult(ServiceResult.Ok("Profile updated successfully!"));
        }
    }
}