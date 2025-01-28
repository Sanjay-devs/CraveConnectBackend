using System.ComponentModel.DataAnnotations;

namespace Test.Entity
{
    public class UserEntity
    {
        [Key]
        public int UserId { get; set; }
        public int UserTypeId { get; set; }
        [Required]
        public string? EmailId { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? VerifyPassword { get; set; }
        public string? UserName { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class UserMasterEntity : UserEntity
    {
        [Key]
        public int UserId { get; set; }
        public string? EmailId { get; set; }
        public string? Password { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class LoginRequestEntity
    {
        public string? EmailId { get; set; }
        public string? Password { get; set; }
    }

    
}
