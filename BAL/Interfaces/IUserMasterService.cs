using CraveConnect.Entity;
using Test.Entity;
using Test.Model;
using static CraveConnect.Utilities.UserActions;

namespace Test.BAL.Intrfaces
{
    public interface IUserMasterService
    {
        Task<List<DropDownList>> UserTypeDD(string? q = "");
        Task<GenricResponse> AddOrUpdateUserType(UserTypeMasterEntity model);
        UserspModel GetAllUsersPagenation(string? q = "", int pageNumber = 1, int pageSize = 10);
        UserEntity GetBbyId(int id);
        Task<GenricResponse> AddEditUser(UserEntity model);
        UserEntity Register(UserEntity user);
        GenricResponse Login(string email, string password);
        List<UserEntity> GetUsersList();
        bool IsEmailAvailable(string emailId);
        GenricResponse DeleteUser(int id);
    }
}
