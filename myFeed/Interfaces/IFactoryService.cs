namespace myFeed.Interfaces
{
    public interface IFactoryService
    {
        TFactory Create<TFactory>();
    }
}