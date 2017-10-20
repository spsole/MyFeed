using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using myFeed.Repositories.Models;

namespace myFeed.Repositories.Abstractions
{
    public interface ICategoriesRepository
    {
        Task<IOrderedEnumerable<Category>> GetAllAsync();
        Task<Article> GetArticleByIdAsync(Guid guid);
        
        Task RearrangeAsync(IEnumerable<Category> categories);
        Task InsertAsync(Category category);
        Task RemoveAsync(Category category);
        Task UpdateAsync(Category category);

        Task InsertChannelAsync(Category category, Channel channel);
        Task RemoveChannelAsync(Category category, Channel channel);
        Task UpdateChannelAsync(Channel channel);

        Task InsertArticleRangeAsync(Channel channel, IEnumerable<Article> articles);
        Task RemoveArticleRangeAsync(Channel channel, IEnumerable<Article> articles);
        Task UpdateArticleAsync(Article article);
    }
}

