using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using FluentResults;

namespace Quizer.Models.User
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
        public bool IsTemporal { get; set; }
        public string? HubConnectionId { get; set; }
        public string? ControlQuestion1 { get; set; }
        public string? ControlQuestion2 { get; set; }
        public string? ControlQuestion3 { get; set; }
        public string? ControlAnswer1Hash { get; set; }
        public string? ControlAnswer2Hash { get; set; }
        public string? ControlAnswer3Hash { get; set; }

        public Result SetAnswers(IPasswordHasher<ApplicationUser> hasher, string controlAnswer1, string controlAnswer2, string controlAnswer3)
        {
            ControlAnswer1Hash = hasher.HashPassword(this, controlAnswer1);
            var result1 = hasher.VerifyHashedPassword(this, ControlAnswer1Hash, controlAnswer1);
            if (result1 == PasswordVerificationResult.Failed)
            {
                return Result.Fail($"Control answer 1 hashing failure.");
            }

            ControlAnswer2Hash = hasher.HashPassword(this, controlAnswer2);
            var result2 = hasher.VerifyHashedPassword(this, ControlAnswer2Hash, controlAnswer2);
            if (result2 == PasswordVerificationResult.Failed)
            {
                return Result.Fail($"Control answer 1 hashing failure.");
            }

            ControlAnswer3Hash = hasher.HashPassword(this, controlAnswer3);
            var result3 = hasher.VerifyHashedPassword(this, ControlAnswer3Hash, controlAnswer3);
            if (result3 == PasswordVerificationResult.Failed)
            {
                return Result.Fail($"Control answer 3 hashing failure.");
            }

            return Result.Ok();
        }
    }
}