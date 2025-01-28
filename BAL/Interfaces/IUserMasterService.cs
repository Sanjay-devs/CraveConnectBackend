using Test.Entity;

namespace Test.BAL.Intrfaces
{
    public interface IUserMasterService
    {
        UserEntity GetBbyId(int id);
        UserEntity Register(UserEntity user);
        GenricResponse Login(string email, string password);
        List<UserEntity> GetUsersList();
        bool IsEmailAvailable(string emailId);
    }
}
