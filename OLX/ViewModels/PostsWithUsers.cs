using OLX.Models;

namespace OLX.ViewModels
{
    public class PostsWithUsers
    {
        public List<Post> Posts { get; set; } = new();
        public List<Users> Users { get; set; } = new();
        public List<Kosz> Kosz { get; set; } = new();
    }
}