//using Microsoft.AspNetCore.Mvc.Filters;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Text;
//using System.Web.Http.Filters;
//using ActionFilterAttribute = Microsoft.AspNetCore.Mvc.Filters.ActionFilterAttribute;

//namespace Test.Utilities
//{
//    public class ValidateTokenAttribute : ActionFilterAttribute
//    {
//        public override void OnActionExecuting(ActionExecutingContext context)
//        {
//            var token = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

//            if (string.IsNullOrEmpty(token))
//            {
//                context.Result = new UnauthorizedObjectResult(new { StatusCode = 440, StatusMessage = "Session expired. Please log in again." });
//                return;
//            }

//            var handler = new JwtSecurityTokenHandler();
//            try
//            {
//                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

//                if (jwtToken == null || jwtToken.ValidTo < DateTime.UtcNow)
//                {
//                    context.Result = new ObjectResult(new { StatusCode = 440, StatusMessage = "Session expired. Please log in again." })
//                    {
//                        StatusCode = 440
//                    };
//                    return;
//                }
//            }
//            catch (Exception)
//            {
//                context.Result = new ObjectResult(new { StatusCode = 440, StatusMessage = "Session expired. Please log in again." })
//                {
//                    StatusCode = 440
//                };
//            }
//        }
//    }
//}
