using System.ComponentModel.DataAnnotations;

namespace CraveConnect.Entity
{
    public class UserTypeMasterEntity
    {
        [Key]
        public int UserTypeId { get; set; }
        public string? UserTypeName { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive{ get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
