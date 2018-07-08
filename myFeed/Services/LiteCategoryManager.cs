﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using myFeed.Models;
using LiteDB;

namespace myFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(ICategoryManager))]
    public sealed class LiteCategoryManager : ICategoryManager
    {
        private readonly LiteDatabase _liteDatabase;
        
        public LiteCategoryManager(LiteDatabase liteDatabase) => _liteDatabase = liteDatabase;

        public Task<IEnumerable<Category>> GetAll() => Task.Run(() => 
        {
            var collection = _liteDatabase.GetCollection<Category>();
            return collection.FindAll().OrderBy(i => i.Order).AsEnumerable();
        });

        public Task<Article> GetArticleById(Guid guid) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            var category = collection.FindOne(Query.EQ("$.Channels[*].Articles[*]._id", guid));
            return category?.Channels?.SelectMany(i => i.Articles).First(i => i.Id == guid);
        });

        public Task Remove(Category category) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            collection.Delete(i => i.Id == category.Id);
        });

        public Task Update(Category category) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            collection.Update(category);
        });

        public Task Insert(Category category) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            var categories = collection.FindAll().ToList();
            category.Order = !categories.Any() ? 0 : categories.Max(i => i.Order) + 1;
            collection.Insert(category);
        });

        public Task Rearrange(IEnumerable<Category> categories) => Task.Run(() =>
        {
            var orderedCategories = categories.ToList();
            for (var x = 0; x < orderedCategories.Count; x++) orderedCategories[x].Order = x;
            _liteDatabase.GetCollection<Category>().Update(orderedCategories);
        });

        public Task Update(Channel channel) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            var query = Query.EQ("$.Channels[*]._id", channel.Id);
            var category = collection.FindOne(query) ?? throw new InvalidOperationException();

            category.Channels.RemoveAll(i => i.Id == channel.Id);
            category.Channels.Add(channel);
            collection.Update(category);
        });

        public Task Update(Article article) => Task.Run(() =>
        {
            var collection = _liteDatabase.GetCollection<Category>();
            var query = Query.EQ("$.Channels[*].Articles[*]._id", article.Id);
            var category = collection.FindOne(query) ?? throw new InvalidOperationException();

            var channel = category.Channels.First(i => i.Articles.Any(x => x.Id == article.Id));
            channel.Articles.RemoveAll(i => i.Id == article.Id);
            channel.Articles.Add(article);
            collection.Update(category);
        });
    }
}