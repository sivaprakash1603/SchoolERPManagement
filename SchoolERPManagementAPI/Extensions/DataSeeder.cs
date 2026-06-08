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
                Createdat = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)
            };
            context.Users.Add(adminUser);
            context.SaveChanges();
        }
    }
}
