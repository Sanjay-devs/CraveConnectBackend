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
using Microsoft.EntityFrameworkCore;
using Amazon.Runtime.Internal;
using Azure;
using Test.Model;
using Microsoft.Data.SqlClient;
using static CraveConnect.Utilities.UserActions;
using CraveConnect.Entity;

namespace Test.DAL.Repos
{
    public class UserMasterRepo : IUserMasterRepo
    {
        private readonly MyDbContext db;
        public UserMasterRepo(MyDbContext _db)
        {
            db = _db;
        }

        public async Task<List<DropDownList>> UserTypeDD(string? q = "")
        {
            try
            {
                var items = new List<DropDownList>();
                var search = new SqlParameter("q", q == null ? "" : q);
                var u = await db.UserType
                                .Where(a => (a.IsDeleted ?? false) == false &&
                                            (string.IsNullOrEmpty(q) || a.UserTypeName.Contains(q)))
                                .ToListAsync();

                foreach (var obj in u)
                {
                    items.Add(new DropDownList { Name = obj.UserTypeName, Id = obj.UserTypeId });
                }
                return items;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<GenricResponse> AddOrUpdateUserType(UserTypeMasterEntity model)
        {
            var response = new GenricResponse();

            try
            {
                // Validate input model
                if (model == null)
                {
                    response.StatusMessage = "Data can not be empty";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }

                var userType = await db.UserType.Where(m => m.UserTypeName == model.UserTypeName && m.IsDeleted== false)
                    .FirstOrDefaultAsync();
                if (userType != null)
                {
                    return new GenricResponse
                    {
                        StatusCode = 409,
                        StatusMessage = "User type already exists.",
                    };
                }

                var userTypeItem = await db.UserType.FirstOrDefaultAsync(m => m.UserTypeId == model.UserTypeId);

                if (userTypeItem != null)
                {
                    // Update existing menu item
                    userTypeItem.UserTypeName = model.UserTypeName;
                    userTypeItem.UserTypeId = model.UserTypeId;
                    userTypeItem.IsDeleted = model.IsDeleted;
                    userTypeItem.IsActive = model.IsActive;

                    db.UserType.Update(userTypeItem);
                    response.StatusMessage = "user type updated successfully.";
                }
                else
                {
                    // Add a new menu item
                    var newMenuItem = new UserTypeMasterEntity
                    {
                        UserTypeId = model.UserTypeId,
                        UserTypeName = model.UserTypeName,
                        IsDeleted = false,
                        IsActive= true, 
                        CreatedOn = DateTime.Now
                    };

                    await db.UserType.AddAsync(newMenuItem);
                    response.StatusMessage = "user type added successfully.";
                }

                // Save changes to the database
                await db.SaveChangesAsync();


            }
            catch (Exception ex)
            {
                // Handle exceptions
                response.StatusMessage = $"An error occurred: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }

            return response;
        }
        public UserspModel GetAllUsersPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            UserspModel response = new UserspModel();
            List<UserModel> myList = new List<UserModel>();

            try
            {
                SqlParameter[] sParams =
                {
                    new SqlParameter("@q", q ?? ""),
                    new SqlParameter("@pageNumber", pageNumber),
                    new SqlParameter("@pageSize", pageSize)
                };

                string sp = "EXEC sp_getUserName @q, @pageNumber, @pageSize";

                // Execute stored procedure to fetch paginated restaurant data
                myList = db.Set<UserModel>().FromSqlRaw(sp, sParams).AsEnumerable().ToList();

                // Fixing the parameter issue in count execution
                SqlParameter[] sParamsCnt =
                {
                    new SqlParameter("@q", q ?? "")
                };

                string spCnt = "EXEC sp_getUserCount @q";

                // Fetch count and totalCount in one call
                DBCountResponse count = db.Set<DBCountResponse>().FromSqlRaw(spCnt, sParamsCnt).AsEnumerable().FirstOrDefault();

                response.count = count?.cnt ?? 0;
                response.totalCount = count?.totalCount ?? 0;  // This should now work
                response.result = myList;
                response.pageNumber = pageNumber;
                response.pageSize = pageSize;
                response.q = q;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching restaurant data", ex);
            }

            return response;
        }
        public UserEntity Register(UserEntity user)
        {
            try
            {
                GenricResponse res = new GenricResponse();
                PasswordHelper pwd = new PasswordHelper();

                //var u = db.Users.Where(a => a.EmailId.Trim().Equals(user.EmailId.Trim()) && a.IsDeleted == false).FirstOrDefault();
                var existuser = db.Users.Where(u => (u.UserName == user.UserName ||
                                                 u.EmailId == user.EmailId ||
                                                 u.MobileNumber == user.MobileNumber)
                                                 && u.IsDeleted == false).Any();

                if (existuser != null)
                {
                    res.StatusCode = 403;
                    res.StatusMessage = "User with this Email exists";
                }
                if (existuser != null && user.Password != user.VerifyPassword)
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
                        user.UserTypeId = user.UserTypeId;
                        user.Password = pwd.Encrypt(user.Password);
                        user.VerifyPassword = pwd.Encrypt(user.VerifyPassword);
                        user.UserName = user.UserName;
                        user.MobileNumber = user.MobileNumber;
                        user.Address = user.Address;
                        user.Image = user.Image;
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
            catch (Exception e)
            {

                throw e;
            }
        }

        //Included updation logic in Registermethod

        //public UserEntity Register(UserEntity user)
        //{
        //    GenricResponse res = new GenricResponse();
        //    PasswordHelper pwd = new PasswordHelper();

        //    var existuser = db.Users
        //        .Where(u => (u.UserName == user.UserName ||
        //                     u.EmailId == user.EmailId ||
        //                     u.MobileNumber == user.MobileNumber)
        //                     && !u.IsDeleted && u.UserId != user.UserId)
        //        .FirstOrDefault();

        //    if (existuser != null)
        //    {
        //        res.StatusCode = 409;
        //        res.StatusMessage = "User with this Email, Username, or Mobile Number already exists";
        //        return null;
        //    }

        //    var existingUser = db.Users.FirstOrDefault(u => u.UserId == user.UserId);

        //    if (existingUser != null)
        //    {
        //        // Update existing user details
        //        existingUser.UserName = user.UserName;
        //        existingUser.EmailId = user.EmailId;
        //        existingUser.UserTypeId = user.UserTypeId;
        //        existingUser.Image = user.Image;
        //        existingUser.MobileNumber = user.MobileNumber;
        //        existingUser.Address = user.Address;
        //        existingUser.IsDeleted = user.IsDeleted;
        //        existingUser.IsActive = user.IsActive;
        //        existingUser.UpdatedOn = DateTime.UtcNow;

        //        db.Users.Update(existingUser);
        //        db.SaveChanges();

        //        res.StatusCode = 200;
        //        res.StatusMessage = "User updated successfully";
        //        return existingUser;
        //    }
        //    else
        //    {
        //        if (user.UserId == 0)
        //        {
        //            // Encrypt password
        //            user.Password = pwd.Encrypt(user.Password);
        //            user.VerifyPassword = pwd.Encrypt(user.VerifyPassword);
        //            user.IsDeleted = false;
        //            user.CreatedOn = DateTime.UtcNow;

        //            db.Users.Add(user);
        //            db.SaveChanges();

        //            res.StatusCode = 200;
        //            res.StatusMessage = "User registered successfully";
        //            return user;
        //        }
        //    }

        //    return null;
        //}



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

        public async Task<GenricResponse> AddEditUser(UserEntity model)
        {
            var response = new GenricResponse();

            try
            {
                // Validate input model
                if (model == null)
                {
                    response.StatusMessage = "Empty data.";
                    response.StatusCode = 400; // Bad Request
                    return response;
                }

                // Check if the user already exists, but ignore the current user if updating
                var existuser = db.Users
                   .Where(u => !u.IsDeleted &&
                               (u.UserName != null && u.UserName == model.UserName ||
                                u.EmailId != null && u.EmailId == model.EmailId ||
                                u.MobileNumber != null && u.MobileNumber == model.MobileNumber)).Any();

                if (existuser != null)
                {
                    return new GenricResponse
                    {
                        StatusCode = 409,
                        StatusMessage = "User already exists",
                    };
                }

                // Check if the user exists for update
                var existingUser = await db.Users.FirstOrDefaultAsync(m => m.UserId == model.UserId);

                if (existingUser != null)
                {
                    // Update existing user
                    existingUser.UserName = model.UserName;
                    existingUser.EmailId = model.EmailId;
                    existingUser.UserTypeId = model.UserTypeId;
                    existingUser.Image = model.Image;
                    existingUser.MobileNumber = model.MobileNumber;
                    existingUser.Address = model.Address;
                    existingUser.IsDeleted = model.IsDeleted;
                    existingUser.IsActive = model.IsActive;
                    existingUser.UpdatedOn = DateTime.UtcNow;

                    db.Users.Update(existingUser);
                    response.StatusCode = 200;
                    response.StatusMessage = "User updated successfully.";
                }
                else
                {
                    // Add a new user
                    var user = new UserEntity
                    {
                        UserName = model.UserName,
                        EmailId = model.EmailId,
                        UserTypeId = await db.UserType
                            .Where(a => a.UserTypeName == "Customer")
                            .Select(b => b.UserTypeId)
                            .FirstOrDefaultAsync(),
                        Image = model.Image,
                        IsDeleted = model.IsDeleted,
                        IsActive = model.IsActive,
                        MobileNumber = model.MobileNumber,
                        Address = model.Address,
                        CreatedOn = DateTime.UtcNow
                    };

                    await db.Users.AddAsync(user);
                    response.StatusCode = 201;
                    response.StatusMessage = "User added successfully.";
                }

                // Save changes to the database
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                response.StatusMessage = $"An error occurred: {ex.Message}";
                response.StatusCode = 500; // Internal Server Error
            }

            return response;
        }



        //public bool IsEmailAvailable(string emailId)
        //{
        //    bool res = false;
        //    var check = db.Users.Where(a => a.IsDeleted == false && a.EmailId==emailId).Any();
        //    if (check != null)
        //    {
        //        res = true;
        //    }
        //    return res;
        //}
        public bool IsEmailAvailable(string emailId)
        {
            return !db.Users.Any(a => a.IsDeleted == false && a.EmailId == emailId);
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

        public GenricResponse DeleteUser(int id)
        {
            GenricResponse resp = new GenricResponse();

            try
            {
                var user = db.Users.FirstOrDefault(r => !r.IsDeleted && r.UserId == id);

                if (user != null)
                {
                    user.IsDeleted = true;
                    db.SaveChanges();

                    resp.StatusCode = 200;
                    resp.StatusMessage = "User Deleted Successfully";
                    resp.CurrentId = id;
                }
                else
                {
                    resp.StatusCode = 404;
                    resp.StatusMessage = "User not found";
                    resp.CurrentId = id;
                }
            }
            catch (Exception)
            {
                resp.StatusCode = 500;
                resp.StatusMessage = "Failed to delete";
            }

            return resp;
        }



    }
}
