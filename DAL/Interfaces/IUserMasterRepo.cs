using Microsoft.AspNetCore.Mvc.Filters;
using Test.Entity;

namespace Test.DAL.Interface
{
    public interface IUserMasterRepo
    {
        UserEntity GetBbyId(int id);
        UserEntity Register(UserEntity user);
        GenricResponse Login(string email, string password);
        bool IsEmailAvailable(string emailId);
        List<UserEntity> GetUsersList();
    }
}
