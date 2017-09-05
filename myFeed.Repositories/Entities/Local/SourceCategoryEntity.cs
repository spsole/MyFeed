using System;
using System.Collections.Generic;

namespace myFeed.Repositories.Entities.Local {
    public class SourceCategoryEntity {
        public Guid Id { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }
        public virtual ICollection<SourceEntity> Sources { get; set; }
        public SourceCategoryEntity() => Sources = new HashSet<SourceEntity>();
    }
}
