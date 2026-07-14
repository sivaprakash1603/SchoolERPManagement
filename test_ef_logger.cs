using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

public class MyContext : DbContext
{
    public MyContext(DbContextOptions options) : base(options) {}
    public void Test()
    {
        var logger = this.GetService<ILogger<MyContext>>();
    }
}
