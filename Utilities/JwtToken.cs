using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Test.Utilities
{
    public class JwtToken : IJwtToken
    {
        private readonly IConfiguration config;
        public JwtToken(IConfiguration _config)
        {
            config = _config;   
        }

        public async Task<bool> checkTokenExpired([FromQuery] string token)
        {

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be null or empty", nameof(token));
            }

            try
            {
                // Decode the token
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Extract the "exp" claim (expiration time)
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                if (expClaim == null)
                {
                    throw new Exception("The token does not contain an 'exp' claim.");
                }

                // Convert "exp" claim from Unix timestamp to DateTime
                var expirationTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;

                // Check if the token is expired
                return DateTime.UtcNow >= expirationTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while validating token: {ex.Message}");
                throw;
            }
        }


        public async Task<string> generateJWToken(string email, string password)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define claims
            var claims = new[]
            {
               new Claim(JwtRegisteredClaimNames.Sub, email), // Email claim
               new Claim("password", password),              // Password claim
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique identifier
                new Claim(JwtRegisteredClaimNames.Iat,
                         new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                         ClaimValueTypes.Integer64) // Issued at
       };

            // Create the token
            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials);

            // Return serialized token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
