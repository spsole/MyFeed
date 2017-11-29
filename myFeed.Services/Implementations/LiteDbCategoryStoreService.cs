using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;

namespace myFeed.Services.Implementations
{
    public sealed class LiteDbCategoryStoreService : ICategoryStoreService
    {
        private readonly LiteDatabase _liteDatabase;
        
        public LiteDbCategoryStoreService(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public Task<IOrderedEnumerable<Category>> GetAllAsync() => Task.Run(() => _liteDatabase
            .GetCollection<Category>().FindAll().OrderBy(i => i.Order));

        public Task<Article> GetArticleByIdAsync(Guid guid) => Task.Run(() => _liteDatabase
            .GetCollection<Category>().FindOne(Query.EQ("$.Channels[*].Articles[*]._id", guid))?
            .Channels?.SelectMany(i => i.Articles).First(i => i.Id == guid));

        public Task RemoveAsync(Category category) => Task.Run(() => _liteDatabase
            .GetCollection<Category>().Delete(i => i.Id == category.Id));

        public Task UpdateAsync(Category category) => Task.Run(() => _liteDatabase
            .GetCollection<Category>().Update(category));

        public Task InsertAsync(Category category) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            var categories = collection.FindAll().ToList();
            category.Order = !categories.Any() ? 0 : categories.Max(i => i.Order) + 1;
            collection.Insert(category);
        });

        public Task RearrangeAsync(IEnumerable<Category> categories) => Task.Run(() =>
        {
            var orderedCategories = categories.ToList();
            for (var x = 0; x < orderedCategories.Count; x++) orderedCategories[x].Order = x;
            _liteDatabase.GetCollection<Category>().Update(orderedCategories);
        });

        public Task InsertChannelAsync(Category category, Channel channel) => Task.Run(() =>
        {
            category.Channels.Add(channel);
            _liteDatabase.GetCollection<Category>().Update(category);
        });

        public Task RemoveChannelAsync(Category category, Channel channel) => Task.Run(() =>
        {
            category.Channels.Remove(channel);
            _liteDatabase.GetCollection<Category>().Update(category);
        });

        public Task UpdateChannelAsync(Channel channel) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            var query = Query.EQ("$.Channels[*]._id", channel.Id);
            var category = collection.FindOne(query);
            if (category == null) return;

            category.Channels.RemoveAll(i => i.Id == channel.Id);
            category.Channels.Add(channel);
            collection.Update(category);
        });

        public Task InsertArticleRangeAsync(Channel channel, IEnumerable<Article> articles) => Task.Run(() =>
        {
            channel.Articles.AddRange(articles);
            return UpdateChannelAsync(channel);
        });

        public Task UpdateArticleAsync(Article article) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            var query = Query.EQ("$.Channels[*].Articles[*]._id", article.Id);
            var category = collection.FindOne(query);
            if (category == null) return;

            var channel = category.Channels.First(i => i.Articles.Any(x => x.Id == article.Id));
            channel.Articles.RemoveAll(i => i.Id == article.Id);
            channel.Articles.Add(article);
            collection.Update(category);
        });
    }
}