using System.Security.Cryptography;
using System.Text;

namespace SimpleVote.Server.Utils;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        using var hmac = new HMACSHA256();
        var bytes = Encoding.UTF8.GetBytes(password);
        var salt = hmac.Key;

        var computed = hmac.ComputeHash(bytes);
        var base64hash = Convert.ToBase64String(computed);
        var base64Salt = Convert.ToBase64String(salt);
        return $"{base64hash}.+{base64Salt}";
    }

    public static bool VerifyHash(string hashed, string password)
    {
        var split = hashed.Split(".+");
        if (split.Length != 2) return false;

        using var hmac = new HMACSHA256(Convert.FromBase64String(split[1]));

        var passwordBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashedBytes = Convert.FromBase64String(split[0]);
        
        return passwordBytes.SequenceEqual(hashedBytes);
    }
}