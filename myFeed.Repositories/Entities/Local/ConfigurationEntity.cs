using System;

namespace myFeed.Repositories.Entities.Local
{
    public class ConfigurationEntity
    {
        public Guid Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}