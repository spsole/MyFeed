using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyFeed.Models;

namespace MyFeed.Interfaces
{
    public interface ICategoryManager
    {
        Task<IEnumerable<Category>> GetAll();

        Task<Article> GetArticleById(Guid guid);
        
        Task Rearrange(IEnumerable<Category> categories);

        Task Insert(Category category);

        Task Remove(Category category);

        Task Update(Category category);

        Task Update(Channel channel);

        Task Update(Article article);
    }
}

