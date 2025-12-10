using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OLX.Models;

namespace OLX.Data
{
    public class OLX2Context : IdentityDbContext<Users>
    {
        public OLX2Context(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Kosz> Kosz { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Paragon> Paragons { get; set; }
    }
}
