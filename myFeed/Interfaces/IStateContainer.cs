namespace myFeed.Interfaces
{
    public interface IStateContainer
    {
        TModel Pop<TModel>();
    }
}