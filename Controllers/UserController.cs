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

        [HttpGet("GetById")]
        public IActionResult GetUserById(int Id)
        {
            var res = service.GetBbyId(Id);
            if (res == null)
            {
                return NotFound(new { message = $"User with ID {Id} not found." });
            }
            return Ok(res);
        }

        [Authorize]
        [HttpGet("GetUsersList")]
        public IActionResult GetUsersList()
        {
            var res = service.GetUsersList();
            return Ok(res);
        }

        [HttpPost("Register")]
        public IActionResult Register(UserEntity user)
        {
            try
            {
                bool isEmailAvailable = service.IsEmailAvailable(user.EmailId);
                if (!isEmailAvailable)
                {
                    return BadRequest(new { StatusCode = 403, StatusMessage = "User exists with this email" });
                }

                var result = service.Register(user);
                if (user.Password != user.VerifyPassword)
                {
                    return BadRequest(new { StatusCode = 0, StatusMessage = "Password Mismatch" });
                }
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
                            userId = user.UserId // Add userId to response
                            
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
                    res.StatusMessage = "Invalid credentials";

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

        


    }
}
