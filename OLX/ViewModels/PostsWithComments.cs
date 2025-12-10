using OLX.Models;

namespace OLX.ViewModels
{
    public class PostsWithComments
    {
        public List<Post> Posts { get; set; } = new();
        public List<Kosz> Kosz { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
    }
}