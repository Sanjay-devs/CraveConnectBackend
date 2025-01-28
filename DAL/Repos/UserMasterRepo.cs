using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Azure.Core;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Test.Context;
using Test.Entity;
using Test.Utilities;
using Test.DAL.Interface;

namespace Test.DAL.Repos
{
    public class UserMasterRepo : IUserMasterRepo
    {
        private readonly MyDbContext db;
        public UserMasterRepo(MyDbContext _db)
        {
            db = _db;
        }


        public UserEntity Register(UserEntity user)
        {
            GenricResponse res = new GenricResponse();
            PasswordHelper pwd = new PasswordHelper();

            var u = db.Users.Where(a => a.EmailId.Trim().Equals(user.EmailId.Trim()) && a.IsDeleted == false).FirstOrDefault();
            if (u != null && u.EmailId == user.EmailId)
            {
                res.StatusCode = 403;
                res.StatusMessage = "User with this Email exists";
            }
            if (u != null && u.Password != user.VerifyPassword)
            {
                res.StatusCode = 0;
                res.StatusMessage = "Password Mismatch";
            }
            else
            {
                if (user.UserId == 0)
                {
                    // Encrypt password
                    user.EmailId = user.EmailId;
                    user.Password = pwd.Encrypt(user.Password);
                    user.VerifyPassword = pwd.Encrypt(user.VerifyPassword);
                    user.UserName = user.UserName;
                    user.IsDeleted = false;
                    user.CreatedOn = DateTime.Now;

                    db.Users.Add(user);
                    db.SaveChanges();

                    res.StatusCode = 200;
                    res.StatusMessage = "User Registered Successfully";
                }
            }

            return user;
        }

        public GenricResponse Login(string email, string password)
        {
            GenricResponse res = new GenricResponse();
            PasswordHelper pwd = new PasswordHelper();

            try
            {
                // Fetch the user from the database
                var user = db.Users.FirstOrDefault(u => u.EmailId == email && u.IsDeleted == false);

                if (user != null)
                {
                    var decryptedPassword = pwd.Decrypt(user.Password); // Decrypt stored password

                    if (decryptedPassword == password)
                    {
                        res.StatusCode = 200;
                        res.StatusMessage = "Logged in successfully";
                    }
                    else
                    {
                        res.StatusCode = 0;
                        res.StatusMessage = "Invalid credentials";
                    }
                }
                else
                {
                    res.StatusCode = 0;
                    res.StatusMessage = "Invalid credentials";
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = 0;
                res.StatusMessage = "An error occurred: " + ex.Message;
            }

            return res;
        }


        public bool IsEmailAvailable(string emailId)
        {
            bool res = true;
            var check = db.Users.Where(a => a.IsDeleted == false && a.EmailId.Trim().Equals(emailId.Trim())).FirstOrDefault();
            if (check != null)
            {
                res = false;
            }
            return res;
        }

        public UserEntity GetBbyId(int id)
        {
            var res = db.Users.Where(a => a.IsDeleted == false && a.UserId == id).FirstOrDefault();

            try
            {
                if (res == null)
                {
                    throw new Exception("User not found");

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return res;

        }

        public List<UserEntity> GetUsersList()
        {
            var users = db.Users.Where(a => a.IsDeleted == false).
                Select(a => new UserEntity
                {
                    UserName = a.UserName,
                    EmailId = a.EmailId,

                }).ToList();

            return users;
        }



    }
}
