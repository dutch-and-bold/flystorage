using System.Collections.Generic;
using System.Linq;

namespace DutchAndBold.Flystorage.Abstractions.Models
{
    public class Config
    {
        public const string OptionVisibility = "visibility";

        public const string OptionDirectoryVisibility = "directory_visibility";

        private readonly Dictionary<string, object> _options;

        public Config(Dictionary<string, object> options = null)
        {
            _options = options ?? new Dictionary<string, object>();
        }

        public T Get<T>(string property, T @default = default)
        {
            return _options.TryGetValue(property, out var result) ? (T)result : @default;
        }

        public Config Extend(Dictionary<string, object> options)
        {
            return new(new List<Dictionary<string, object>>
                {
                    options,
                    _options
                }
                .SelectMany(dict => dict)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(group => group.Key, group => group.First()));
        }

        public Config WithDefaults(Dictionary<string, object> defaults)
        {
            return new(new List<Dictionary<string, object>>
                {
                    _options,
                    defaults
                }
                .SelectMany(dict => dict)
                .ToLookup(pair => pair.Key, pair => pair.Value)
                .ToDictionary(group => group.Key, group => group.First()));
        }
    }
}