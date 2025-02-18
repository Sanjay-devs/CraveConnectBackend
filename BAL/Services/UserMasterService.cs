using CraveConnect.Entity;
using Test.BAL.Intrfaces;
using Test.Context;
using Test.DAL.Interface;
using Test.Entity;
using Test.Model;
using static CraveConnect.Utilities.UserActions;

namespace Test.BAL.Services
{
    public class UserMasterService : IUserMasterService
    {
        private readonly IUserMasterRepo repo;
        private readonly MyDbContext db;

        public UserMasterService(IUserMasterRepo _repo, MyDbContext _db)
        {
            repo = _repo;
            db = _db;
        }
        public Task<List<DropDownList>> UserTypeDD(string? q = "")
        {
            return repo.UserTypeDD(q);
        }
        public Task<GenricResponse> AddOrUpdateUserType(UserTypeMasterEntity model)
        {
            return repo.AddOrUpdateUserType(model);
        }
        public UserspModel GetAllUsersPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            return repo.GetAllUsersPagenation(q, pageNumber, pageSize);
        }
        public UserEntity GetBbyId(int id)
        {
            return repo.GetBbyId(id);
        }
        public Task<GenricResponse> AddEditUser(UserEntity model)
        {
            return repo.AddEditUser(model);
        }
        public UserEntity Register(UserEntity user)
        {
            return repo.Register(user);
        }

        public GenricResponse Login(string email, string password)
        {
            return repo.Login(email, password);
        }

        public List<UserEntity> GetUsersList()
        {
            return repo.GetUsersList();
        }

        public bool IsEmailAvailable(string emailId)
        {
            return repo.IsEmailAvailable(emailId);
        }

        public GenricResponse DeleteUser(int id)
        {
            return repo.DeleteUser(id);
        }

    }
}
