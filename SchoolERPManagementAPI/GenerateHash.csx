using System;
using System.Security.Cryptography;

class Program
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    static void Main()
    {
        var password = "Admin@123";
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        var hash = $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(key)}";
        
        Console.WriteLine($"UPDATE users SET passwordhash = '{hash}' WHERE username = 'admin';");
    }
}
Main();
