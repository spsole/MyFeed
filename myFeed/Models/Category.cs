using System;
using System.Collections.Generic;
using LiteDB;

namespace myFeed.Models
{
    public sealed class Category
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public int Order { get; set; }
        
        public string Title { get; set; }
        
        public List<Channel> Channels { get; set; }
        
        public Category() => Channels = new List<Channel>();
    }
}