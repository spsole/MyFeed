namespace myFeed.Services.Abstractions
{
    public interface IFactoryService
    {
        TObject CreateInstance<TObject>(params object[] arguments);
    }
}