using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using myFeed.ViewModels;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;
using DryIoc.MefAttributedModel;
using DryIoc;
using LiteDB;

namespace myFeed.Uwp
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
            var articleFactory = _container.Resolve<Func<FeedItemViewModel, FeedItemFullViewModel>>();
            var navigationService = _container.Resolve<INavigationService>();
            var articleViewModel = articleFactory(factory(article));

            if (navigationService.CurrentViewModelType != typeof(FeedViewModel))
            {
                await navigationService.Navigate<FeedViewModel>();
                await Task.Delay(150);
            }
            await navigationService.NavigateTo(articleViewModel);
        }
    }
}
