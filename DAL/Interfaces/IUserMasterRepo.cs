using CraveConnect.Entity;
using Microsoft.AspNetCore.Mvc.Filters;
using Test.Entity;
using Test.Model;
using static CraveConnect.Utilities.UserActions;

namespace Test.DAL.Interface
{
    public interface IUserMasterRepo
    {
        Task<List<DropDownList>> UserTypeDD(string? q = "");
        Task<GenricResponse> AddOrUpdateUserType(UserTypeMasterEntity model);
        UserspModel GetAllUsersPagenation(string? q = "", int pageNumber = 1, int pageSize = 10);
        UserEntity GetBbyId(int id);
        Task<GenricResponse> AddEditUser(UserEntity model);
        UserEntity Register(UserEntity user);
        GenricResponse Login(string email, string password);
        bool IsEmailAvailable(string emailId);
        List<UserEntity> GetUsersList();
        GenricResponse DeleteUser(int id);
    }
}
