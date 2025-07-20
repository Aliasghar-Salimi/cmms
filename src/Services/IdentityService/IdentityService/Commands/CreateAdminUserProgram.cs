using IdentityService.Commands;

namespace IdentityService;

public class CreateAdminUserProgram
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 CMMS Identity Service - Create Admin User");
        Console.WriteLine("=============================================");
        
        var connectionString = "Server=localhost;Database=CMMSIdentityService;User=sa;Password=Ali@1234;TrustServerCertificate=True";
        
        if (args.Length > 0)
        {
            connectionString = args[0];
        }
        
        try
        {
            await CreateAdminUser.CreateAdminAsync(connectionString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("Make sure your database is running and connection string is correct.");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
} 