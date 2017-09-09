using System;
using System.Collections.Generic;

namespace myFeed.Repositories.Entities.Local
{
    public class SourceEntity
    {
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public bool Notify { get; set; }
        public virtual SourceCategoryEntity Category { get; set; }
        public virtual ICollection<ArticleEntity> Articles { get; set; }
        public SourceEntity() => Articles = new HashSet<ArticleEntity>();
    }
}