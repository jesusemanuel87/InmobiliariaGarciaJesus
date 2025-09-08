using System;

namespace InmobiliariaGarciaJesus.Utils
{
    public class HashGenerator
    {
        public static void GenerateAdminHash()
        {
            string password = "admin123";
            
            // Generate a proper BCrypt hash for admin123
            string correctHash = BCrypt.Net.BCrypt.HashPassword(password);
            
            Console.WriteLine("=== ADMIN PASSWORD HASH GENERATOR ===");
            Console.WriteLine($"Password: {password}");
            Console.WriteLine($"Generated BCrypt Hash: {correctHash}");
            Console.WriteLine();
            
            // Verify the hash works
            bool isValid = BCrypt.Net.BCrypt.Verify(password, correctHash);
            Console.WriteLine($"Hash verification test: {(isValid ? "SUCCESS" : "FAILED")}");
            Console.WriteLine();
            Console.WriteLine("Copy the hash above to update the create_admin_user.sql script");
        }
    }
}
