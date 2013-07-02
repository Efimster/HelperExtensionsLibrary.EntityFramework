using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HelperExtensionsLibrary.Strings;
using HelperExtensionsLibrary.IEnumerable;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public class QueryBuilder
    {
        public List<KeyValueQuery> Queries { get; private set; }

        private QueryBuilder() { Queries = new List<KeyValueQuery>(); }

        public static QueryBuilder Parse(string filter)
        {
            var builder = new QueryBuilder();
            filter.SplitExt("and").ForEach(entry =>
            {
                
                if (entry.Contains("like"))
                {
                    var keyValue = entry.Split(new[]{"like"}, StringSplitOptions.RemoveEmptyEntries);
                    builder.Queries.Add(new LikeQuery(keyValue[0], keyValue[1]));
                }
                else if (entry.Contains("="))
                {
                    var keyValue = entry.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    builder.Queries.Add(new KeyValueQuery(keyValue[0], keyValue[1]));
                }

            });

            return builder;
        }

    }



    public class KeyValueQuery
    {
        public string Key { get; protected set; }
        public string Value { get; protected set; }
        public string Sign { get; protected set; }

        public KeyValueQuery(string key, string value)
        {
            Key = key.Trim(' ');
            Value = value.Trim(' ','\'');
            Sign = "=";
        }

        public virtual bool Filter(IDictionary<string, dynamic> data)
        {
            if (!data.Any(prop => prop.Key == Key && prop.Value.ToString() == Value.Trim('\'')))
                return false;

            return true;
        }
    }

    public class LikeQuery : KeyValueQuery
    {
        public string Pattern { get; protected set; }

        public LikeQuery(string key, string value) : base(key, value)
        {
            Pattern = Value.Replace("%", "(\\w*)");
        }

        public override bool Filter(IDictionary<string, dynamic> data)
        {

            if (!data.Any(prop => prop.Key == Key && Regex.IsMatch(prop.Value.ToString(), Pattern)))
                    return false;

            return true;
        }
    }


}
