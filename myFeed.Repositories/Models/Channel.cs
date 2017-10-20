using System;
using System.Collections.Generic;
using LiteDB;

namespace myFeed.Repositories.Models
{
    public sealed class Channel
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [BsonField]
        public string Uri { get; set; }
        
        [BsonField]
        public bool Notify { get; set; }
        
        [BsonField]
        public List<Article> Articles { get; set; }
        
        public Channel() => Articles = new List<Article>();
    }
}