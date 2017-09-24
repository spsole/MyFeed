using myFeed.ViewModels.Extensions;

namespace myFeed.ViewModels.Implementations
{
    public sealed class ArticleViewModel
    {
        public ArticleViewModel() => Article = new Property<FeedItemViewModel>();

        /// <summary>
        /// Article ViewModel represented as feed item ViewModel.
        /// </summary>
        public Property<FeedItemViewModel> Article { get; }
    }
}
