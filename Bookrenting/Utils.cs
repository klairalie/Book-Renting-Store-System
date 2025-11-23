using System;
using Microsoft.AspNetCore.Identity;

namespace BookRentingUtils
{
    public static class Utils
    {
        public static string HashPassword(string password)
        {
            var hasher = new PasswordHasher<string>();
            return hasher.HashPassword(null, password);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run -- <password>");
                return;
            }

            var hash = Utils.HashPassword(args[0]);
            Console.WriteLine($"Hashed password: {hash}");
        }
    }
}
