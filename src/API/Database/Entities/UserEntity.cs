using Microsoft.AspNetCore.Identity;

namespace API.Database.Entities
{
    public class UserEntity : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string RefreshToken { get; set; }

        public DateTime RefreshTokenExpiration { get; set; }

        public string ResetToken { get; set; }
        
        public DateTime? ResetTokenExpiration { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
