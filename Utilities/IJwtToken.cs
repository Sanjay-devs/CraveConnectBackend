using Microsoft.AspNetCore.Mvc;

namespace Test.Utilities
{
    public interface IJwtToken
    {
        Task<bool> checkTokenExpired([FromQuery] string token);
        Task<string> generateJWToken(string email, string password);
    }
}
