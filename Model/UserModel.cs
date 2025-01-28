using System.ComponentModel.DataAnnotations;

namespace Test.Model
{
    public class UserModel
    {
        public int Id { get; set; }
        public string? EmailId { get; set; }
        public string? Password { get; set; }
        public string? VerifyPassword { get; set; }
        public string? Name { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
