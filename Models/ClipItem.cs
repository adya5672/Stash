using System;
using System.Windows.Media;


namespace stash.Models
{
    public enum ClipType
    {
        Text, 
        Code, 
        Url,
        Image
    }

    public class ClipItem
    {
        public Guid Id { get; set; }
        public ClipType ClipType { get; set; }
        public string? Content {  get; set; }
        public byte[]? ImageData { get; set; }
        public DateTime CopiedAt { get; set;  }
        public bool Ispinned { get; set;  }
        public string? Source { get; set; }

        public ClipItem()
        {
            Id = Guid.NewGuid();
            CopiedAt = DateTime.Now;
        }
    }
}
