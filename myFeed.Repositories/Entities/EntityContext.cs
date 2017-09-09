using myFeed.Repositories.Entities.Local;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace myFeed.Repositories.Entities
{
    public class EntityContext : DbContext
    {
        private readonly ILoggerFactory _loggerFactory;

        public EntityContext() { }

        public EntityContext(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

        public DbSet<SourceCategoryEntity> SourceCategories { get; set; }
        public DbSet<ConfigurationEntity> Configuration { get; set; }
        public DbSet<SourceEntity> SourceEntities { get; set; }
        public DbSet<ArticleEntity> Articles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var article = modelBuilder.Entity<ArticleEntity>();
            article.HasKey(i => i.Id);

            var config = modelBuilder.Entity<ConfigurationEntity>();
            config.Property(i => i.Value).IsRequired();
            config.Property(i => i.Key).IsRequired();
            config.HasKey(i => i.Id);

            var source = modelBuilder.Entity<SourceEntity>();
            source.HasKey(i => i.Id);
            source
                .HasMany(i => i.Articles)
                .WithOne(i => i.Source)
                .OnDelete(DeleteBehavior.SetNull);

            var category = modelBuilder.Entity<SourceCategoryEntity>();
            category.HasKey(i => i.Id);
            category.Property(i => i.Order).IsRequired();
            category
                .HasMany(i => i.Sources)
                .WithOne(i => i.Category)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=MyFeed.db;");
            if (_loggerFactory != null)
                optionsBuilder.UseLoggerFactory(_loggerFactory);
            base.OnConfiguring(optionsBuilder);
        }

        public override void Dispose()
        {
            base.Dispose();
            _loggerFactory?.Dispose();
        }
    }
}