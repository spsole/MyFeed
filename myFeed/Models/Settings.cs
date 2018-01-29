using System;
using LiteDB;

namespace myFeed.Models
{
    public sealed class Settings
    {
        [BsonId]
        public Guid Id { get; set; } = new Guid();
        
        public DateTime Fetched { get; set; }
        
        public bool Images { get; set; }
        
        public bool Banners { get; set; }
        
        public string Theme { get; set; }
        
        public double Font { get; set; }
        
        public int Period { get; set; }
        
        public int Max { get; set; }
    }
}