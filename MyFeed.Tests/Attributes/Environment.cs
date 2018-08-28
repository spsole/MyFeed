using LiteDB;

namespace MyFeed.Tests.Attributes
{
    public static class Environment
    {
        public static LiteDatabase Database { get; } = new LiteDatabase("MyFeed.db");
    }
}