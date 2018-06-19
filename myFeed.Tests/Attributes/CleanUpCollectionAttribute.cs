using System;
using System.Reflection;
using Xunit.Sdk;

namespace myFeed.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CleanUpCollectionAttribute : BeforeAfterTestAttribute
    {
        private readonly string _name;

        public CleanUpCollectionAttribute(string name) => _name = name;

        public override void Before(MethodInfo methodUnderTest) => Environment.Database.DropCollection(_name);

        public override void After(MethodInfo methodUnderTest) => Environment.Database.DropCollection(_name);
    }
}