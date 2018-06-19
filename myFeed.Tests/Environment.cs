using LiteDB;

namespace myFeed.Tests
{
    public static class Environment
    {
        public static LiteDatabase Database { get; } = new LiteDatabase("MyFeed.db");
    }
}