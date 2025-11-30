using System;
using Microsoft.AspNetCore.Identity; // For PasswordHasher

namespace BookRentingUtils
{
    public static class Utils
    {
        // Hash a password
        public static string HashPassword(string password)
        {
            var hasher = new PasswordHasher<string>();
            return hasher.HashPassword(string.Empty, password);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Check if user passed a password
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- Nabiadmin123!");
                return;
            }

            // Hash the password
            var hash = Utils.HashPassword(args[0]);
            Console.WriteLine($"Hashed password: {hash}");
        }
    }
}
