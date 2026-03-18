using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Database.Entities
{
    [Table("UsersOtps", Schema = "security")]
    public class UserOtpEntity
    {
        [Key] public Guid Id { get; set; }

        public string UserId { get; set; }

        public string Code { get; set; }

        public bool IsUsed { get; set; }

        public DateTime ExpirationDate { get; set; }
        
        public DateTime CreatedDate { get; set; }

        [ForeignKey(nameof(UserId))] public UserEntity User { get; set; }
    }
}
