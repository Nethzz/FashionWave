using Microsoft.EntityFrameworkCore;
using RegApi.Model;

namespace RegApi.Data
{
    public class ApiDbContext:DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options):base(options)
        {
            
        }
        public DbSet<Registration> Regs { get; set; }
        public DbSet<PersonalDetails> PersonalDetails { get; set; }
        public DbSet<UserCredentials> UserCredentials { get; set; }
        public DbSet<ShopOwner> shopOwners { get; set; }
        public DbSet<AddProducts> AddProducts { get; set; }
        public DbSet<ProductPayMent> productPayMents { get; set; }
        public object RegistrationRequest { get; internal set; }
    }
}
