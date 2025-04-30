public class ReelDto
{
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public string ThumbnailUrl { get; set; } = null!;
}