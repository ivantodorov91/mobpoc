using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Groupyfy.Security.Persistence
{
    public class GroupyfySecurityDbContextFactory : IDesignTimeDbContextFactory<GroupyfySecurityDbContext>
    {
        public GroupyfySecurityDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GroupyfySecurityDbContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=GroupyfySecurity;Trusted_Connection=True;Application Name=GroupyfySecurity;");

            return new GroupyfySecurityDbContext(optionsBuilder.Options);
        }
    }
}
