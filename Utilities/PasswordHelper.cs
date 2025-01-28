using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Test.Utilities
{
    public class PasswordHelper
    {
        
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("MySecretKey12345"); 
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("MySecretKey12345"); 


        public string Encrypt(string plainText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    var plainBytes = Encoding.UTF8.GetBytes(plainText);
                    var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    var cipherBytes = Convert.FromBase64String(cipherText);
                    var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                    return Encoding.UTF8.GetString(plainBytes);
                }
            }
        }
    }
}
