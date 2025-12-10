using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OLX.Models
{
    public class Kosz
    {
        public int Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public int PostId { get; set; }
    }
}
