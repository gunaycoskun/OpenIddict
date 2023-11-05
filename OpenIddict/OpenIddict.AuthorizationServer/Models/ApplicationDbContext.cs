using Microsoft.EntityFrameworkCore;

namespace OpenIddict.AuthorizationServer.Models
{
    public class ApplicationDbContext: DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {
        }
    }
}
