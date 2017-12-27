namespace myFeed.Services.Abstractions
{
    public interface IStateContainer
    {
        TModel Pop<TModel>();
    }
}