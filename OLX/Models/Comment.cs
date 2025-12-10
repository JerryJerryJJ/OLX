namespace OLX.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Text { get; set; }
        public int Post { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    }
}
