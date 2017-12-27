using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Interfaces
{
    public interface ICategoryManager
    {
        Task<IEnumerable<Category>> GetAllAsync();

        Task<Article> GetArticleByIdAsync(Guid guid);
        
        Task RearrangeAsync(IEnumerable<Category> categories);

        Task InsertAsync(Category category);

        Task RemoveAsync(Category category);

        Task UpdateAsync(Category category);

        Task UpdateChannelAsync(Channel channel);

        Task UpdateArticleAsync(Article article);
    }
}

