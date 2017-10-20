using System;
using System.Collections.Generic;
using LiteDB;

namespace myFeed.Repositories.Models
{
    public sealed class Category
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [BsonField]
        public int Order { get; set; }
        
        [BsonField]
        public string Title { get; set; }
        
        [BsonField]
        public List<Channel> Channels { get; set; }
        
        public Category() => Channels = new List<Channel>();
    }
}