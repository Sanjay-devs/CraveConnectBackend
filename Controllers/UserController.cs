using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Test.Context;
using Test.Entity;
using Test.Utilities;
using Azure;
using Test.BAL.Intrfaces;
using System.Net;
using Test.Model;
using CraveConnect.Entity;

namespace Test.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IJwtToken jwtToken;
        private readonly IConfiguration configuration;
        private readonly IUserMasterService service;
        private readonly MyDbContext db;
        //private readonly TokenValidationMiddleware valid;

        public UserController(IUserMasterService _service,
            MyDbContext _db,
            IConfiguration configuration,
            IJwtToken _jwtToken)
        //TokenValidationMiddleware _valid)
        {
            service = _service;
            db = _db;
            this.configuration = configuration;
            jwtToken = _jwtToken;
            //this.valid = _valid;
        }

        [HttpGet("UserTypeDD")]
        public async Task<IActionResult> UserTypeDD(string? q = "")
        {
            var res = await service.UserTypeDD(q);
            return Ok(res);
        }
        [HttpPost("AddOrUpdateUserType")]
        public async Task<ActionResult<GenricResponse>> AddOrUpdateUserType(UserTypeMasterEntity model)
        {

            if (model == null)
            {
                return BadRequest(new GenricResponse
                {
                    StatusCode = 400,
                    StatusMessage = "Invalid request data.",
                });
            }

            else
            {
                if (model != null)
                {
                    var response = await service.AddOrUpdateUserType(model);
                    return StatusCode(response.StatusCode, response);
                }
                return Ok(model);
            }



        }

        [HttpGet("GetAllUsersPagenation")]
        public IActionResult GetAllUsersPagenation(string? q = "", int pageNumber = 1, int pageSize = 10)
        {
            var res = service.GetAllUsersPagenation(q, pageNumber, pageSize);
            return Ok(res);
        }

        [HttpGet("GetUserById")]
        public IActionResult GetUserById(int Id)
        {
            var res = service.GetBbyId(Id);
            if (res == null)
            {
                return NotFound(new { message = $"User with ID {Id} not found." });
            }
            return Ok(res);
        }

        //[Authorize]
        [HttpGet("GetUsersList")]
        public IActionResult GetUsersList()
        {
            var res = service.GetUsersList();
            return Ok(res);
        }

        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser(UserModel user)
        {
            try
            {
                var existingUser = db.Users.FirstOrDefault(u => u.UserId == user.UserId);
                if (existingUser == null)
                {
                    return NotFound(new { StatusCode = 404, StatusMessage = "User not found" });
                }

                // Check for conflicts
                var existUser = db.Users
                    .Where(u => (u.EmailId == user.EmailId ||
                                 u.MobileNumber == user.MobileNumber)
                                && u.UserId != user.UserId && !u.IsDeleted)
                    .FirstOrDefault();

                if (existUser != null)
                {
                    return Conflict(new { StatusCode = 409, StatusMessage = "User with this Email, or Mobile Number already exists" });
                }

                // Update User Details
                existingUser.UserName = user.UserName;
                existingUser.EmailId = user.EmailId;
                existingUser.MobileNumber = user.MobileNumber;
                existingUser.Address = user.Address;
                existingUser.Image = user.Image;
                existingUser.UpdatedOn = DateTime.UtcNow;

                db.Users.Update(existingUser);
                db.SaveChanges();

                return Ok(new { StatusCode = 200, StatusMessage = "User updated successfully", updatedUser = existingUser });

            }
            catch (Exception ex)
            {
                return BadRequest(new { StatusCode = 500, StatusMessage = ex.Message });
            }
        }


        [HttpPost("Register")]
        public IActionResult Register(UserEntity user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest(new { StatusCode = 0, StatusMessage = "User data cannot be null" });
                }
                if (string.IsNullOrWhiteSpace(user.EmailId))
                {
                    return BadRequest(new { StatusCode = 0, StatusMessage = "Email ID is required" });
                }

                // Fix IsEmailAvailable logic
                if (!service.IsEmailAvailable(user.EmailId))
                {
                    return BadRequest(new { StatusCode = 403, StatusMessage = "User exists with this email" });
                }

                

                if (user.Password != user.VerifyPassword)
                {
                    return BadRequest(new { StatusCode = 0, StatusMessage = "Password Mismatch" });
                }

                var result = service.Register(user);
                if (result != null)
                {
                    return Ok(new { StatusCode = 200, StatusMessage = "Registration successful" });
                }
                else
                {
                    return BadRequest(new { StatusCode = 0, StatusMessage = "Registration failed" });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { StatusCode = 0, StatusMessage = ex.Message });
            }
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromQuery] string email, [FromQuery] string password)
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
                        // Generate JWT token
                        var token = await jwtToken.generateJWToken(email, password);

                        res.StatusCode = 200;
                        res.StatusMessage = "Logged in successfully";
                        res.Data = token;

                        return Ok(new
                        {
                            value1 = res,
                            userId = user.UserId,
                            userName = user.UserName,
                            mobileNumber = user.MobileNumber,
                            emailid = user.EmailId,
                            address = user.Address,
                            userTypeId = user.UserTypeId

                        });
                    }
                    else
                    {
                        res.StatusCode = 401;
                        res.StatusMessage = "Invalid credentials";

                        return Unauthorized(res);
                    }
                }
                else
                {
                    res.StatusCode = 401;
                    res.StatusMessage = "Password mismatch";

                    return Unauthorized(res);
                }
            }
            catch (Exception ex)
            {
                res.StatusCode = 500;
                res.StatusMessage = "An error occurred: " + ex.Message;

                return StatusCode(500, res);
            }
        }

        [HttpPost("TokenExpire")]
        public async Task<IActionResult> CheckTokenExpired([FromBody] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Token cannot be null or empty");
            }

            try
            {
                var isExpired = await jwtToken.checkTokenExpired(token);

                return Ok(new { IsExpired = isExpired });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error while validating token: " + ex.Message });
            }
        }

        [HttpDelete("DeleteUser")]
        public IActionResult DeleteUser(int id)
        {
            var response = service.DeleteUser(id); // Now returning GenricResponse

            if (response.StatusCode == 200)
            {
                return Ok(response);
            }
            else if (response.StatusCode == 404)
            {
                return NotFound(response);
            }
            else
            {
                return StatusCode(500, response);
            }
        }



    }
}
