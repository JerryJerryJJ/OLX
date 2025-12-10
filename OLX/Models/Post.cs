using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLX.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
        public bool IsSold { get; set; } = false;
        public byte[]? Picture { get; set; }
        [Required]
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public Users? User { get; set; }
        public bool Aktualne { get; set; } = true;
    }
}
