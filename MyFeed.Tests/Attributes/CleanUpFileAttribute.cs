using System;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace MyFeed.Tests.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class CleanUpFileAttribute : BeforeAfterTestAttribute
    {
        private readonly string _name;

        public CleanUpFileAttribute(string name) => _name = name;

        public override void Before(MethodInfo methodUnderTest) => File.Delete(_name);

        public override void After(MethodInfo methodUnderTest) => File.Delete(_name);
    }
}