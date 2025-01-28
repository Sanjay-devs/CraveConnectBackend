using Test.BAL.Intrfaces;
using Test.Context;
using Test.DAL.Interface;
using Test.Entity;

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

        public UserEntity GetBbyId(int id)
        {
            return repo.GetBbyId(id);
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


    }
}
