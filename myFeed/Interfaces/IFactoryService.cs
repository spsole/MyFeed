namespace myFeed.Interfaces
{
    public interface IFactoryService
    {
        TObject CreateInstance<TObject>(params object[] arguments);
    }
}