using System;
using LiteDB;

namespace myFeed.Services.Models
{
    public sealed class Article
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime PublishedDate { get; set; }

        public string FeedTitle { get; set; }

        public string ImageUri { get; set; }

        public string Content { get; set; }

        public string Title { get; set; }

        public string Uri { get; set; }

        public bool Read { get; set; }

        public bool Fave { get; set; }
    }
}