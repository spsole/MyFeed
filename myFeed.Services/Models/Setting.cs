using System;
using LiteDB;

namespace myFeed.Services.Models
{
    public sealed class Setting
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [BsonField]
        public string Key { get; set; }
        
        [BsonField]
        public string Value { get; set; }
    }
}