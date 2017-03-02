using System.Collections.Generic;
using System.Text.RegularExpressions;
using Lotus.Serialization.Attributes;

namespace Tera.SystemMessages
{
    [Complex]
    public class SystemMessage
    {
        public Dictionary<string, string> Properties { get; set; }

        public string Name { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            string MatchEvaluator(Match m) => Properties[m.Groups[1].Value];

            var content = Regex.Replace(Text, @"{(\w+)(?:@\w+)?}", MatchEvaluator);
            return $"{Name}: {content}";
        }
    }
}