namespace myFeed.Services.Abstractions
{
    public interface IMediationService
    {
        TModel Get<TModel>();

        void Set<TModel>(TModel model);
    }
}