using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using MyFeed.ViewModels;
using MyFeed.Interfaces;
using MyFeed.Models;
using MyFeed.Platform;
using DryIoc.MefAttributedModel;
using DryIoc;
using LiteDB;

namespace MyFeed.Uwp
{
    public class Bootstrapper
    {
        private readonly IContainer _container = new Container();
        
        public MenuViewModel MenuViewModel => _container.Resolve<MenuViewModel>();

        public Bootstrapper()
        {
            var localFolder = ApplicationData.Current.LocalFolder;
            var filePath = Path.Combine(localFolder.Path, "MyFeed.db");
            _container.RegisterDelegate(x => new LiteDatabase(filePath), Reuse.Singleton);
            _container.RegisterExports(new[] {typeof(App).GetAssembly()});
            _container.RegisterShared();
        }

        public async Task NavigateToToast(Guid guid)
        {
            var categoryManager = _container.Resolve<ICategoryManager>();
            var article = await categoryManager.GetArticleById(guid);
            if (article == null) return;

            var factory = _container.Resolve<Func<Article, FeedItemViewModel>>();
            var navigationService = _container.Resolve<INavigationService>();
            var articleViewModel = factory(article);

            if (navigationService.CurrentViewModelType != typeof(FeedViewModel))
            {
                await navigationService.Navigate<FeedViewModel>();
                await Task.Delay(150);
            }
            await navigationService.NavigateTo(articleViewModel);
        }
    }
}
