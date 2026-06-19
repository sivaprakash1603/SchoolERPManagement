using SchoolERPManagementBLLibrary.Helpers;
using SchoolERPManagementDALLibrary.Contexts;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementAPI.Extensions;

public static class DataSeeder
{
    public static void SeedData(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SchoolERPDbContext>();

        var roles = new[] { "Admin", "Teacher", "Student", "Parent" };
        foreach (var role in roles)
        {
            if (!context.Roles.Any(r => r.Rolename == role))
            {
                context.Roles.Add(new Role { Rolename = role });
            }
        }
        
        context.SaveChanges();

        var adminRole = context.Roles.FirstOrDefault(r => r.Rolename == "Admin");
        if (adminRole != null && !context.Users.Any(u => u.Roleid == adminRole.Id))
        {
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@schoolerp.com",
                Passwordhash = PasswordHasher.Hash("admin123"),
                Roleid = adminRole.Id,
                Isactive = true,
                Createdat = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
            };
            context.Users.Add(adminUser);
            context.SaveChanges();
        }

        // Fix mock data hashes
        var dummyUsers = context.Users.Where(u => u.Passwordhash == "hash").ToList();
        if (dummyUsers.Any())
        {
            var validHash = PasswordHasher.Hash("admin123");
            foreach (var u in dummyUsers)
            {
                u.Passwordhash = validHash;
            }
            context.SaveChanges();
        }

        // Update existing classes that have null academicyearid
        var currentYear = context.Academicyears.FirstOrDefault(y => y.Iscurrent == true);
        if (currentYear != null)
        {
            var classesToUpdate = context.Classes.Where(c => c.Academicyearid == null).ToList();
            if (classesToUpdate.Any())
            {
                foreach (var c in classesToUpdate)
                {
                    c.Academicyearid = currentYear.Id;
                }
                context.SaveChanges();
            }
        }
    }
}
