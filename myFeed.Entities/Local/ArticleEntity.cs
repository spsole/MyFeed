using System;

namespace myFeed.Entities.Local
{
    public class ArticleEntity
    {
        public Guid Id { get; set; }
        public DateTime PublishedDate { get; set; }
        public string FeedTitle { get; set; }
        public string ImageUri { get; set; }
        public string Content { get; set; }
        public string Title { get; set; }
        public string Uri { get; set; }
        public bool Read { get; set; }
        public bool Fave { get; set; }
        public virtual SourceEntity Source { get; set; }
    }
}