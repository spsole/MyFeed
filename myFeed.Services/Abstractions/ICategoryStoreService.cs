using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Services.Models;

namespace myFeed.Services.Abstractions
{
    public interface ICategoryStoreService
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Article> GetArticleByIdAsync(Guid guid);
        
        Task RearrangeAsync(IEnumerable<Category> categories);
        Task InsertAsync(Category category);
        Task RemoveAsync(Category category);
        Task UpdateAsync(Category category);

        Task InsertChannelAsync(Category category, Channel channel);
        Task RemoveChannelAsync(Category category, Channel channel);
        Task UpdateChannelAsync(Channel channel);

        Task InsertArticleRangeAsync(Channel channel, IEnumerable<Article> articles);
        Task UpdateArticleAsync(Article article);
    }
}

