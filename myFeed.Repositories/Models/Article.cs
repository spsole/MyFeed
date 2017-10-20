using System;
using LiteDB;

namespace myFeed.Repositories.Models
{
    public sealed class Article
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();

        [BsonField]
        public DateTime PublishedDate { get; set; }

        [BsonField]
        public string FeedTitle { get; set; }

        [BsonField]
        public string ImageUri { get; set; }

        [BsonField]
        public string Content { get; set; }

        [BsonField]
        public string Title { get; set; }

        [BsonField]
        public string Uri { get; set; }

        [BsonField]
        public bool Read { get; set; }

        [BsonField]
        public bool Fave { get; set; }
    }
}