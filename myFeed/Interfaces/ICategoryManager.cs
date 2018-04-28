using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Models;

namespace myFeed.Interfaces
{
    public interface ICategoryManager
    {
        Task<IEnumerable<Category>> GetAll();

        Task<Article> GetArticleById(Guid guid);
        
        Task<bool> Rearrange(IEnumerable<Category> categories);

        Task<bool> Insert(Category category);

        Task<bool> Remove(Category category);

        Task<bool> Update(Category category);

        Task<bool> Update(Channel channel);

        Task<bool> Update(Article article);
    }
}

