using System.Collections.Generic;
using System.Text.RegularExpressions;
using Omnius.Base;

namespace Amoeba.Daemon
{
    static class UriUtils
    {
        private readonly static Regex _regex1 = new Regex(@"(.*?):(.*)", RegexOptions.Compiled);
        private readonly static Regex _regex2 = new Regex(@"(.*):(\d*)", RegexOptions.Compiled);

        public static Information Parse(string uri)
        {
            var match1 = _regex1.Match(uri);

            if (match1.Success)
            {
                string scheme = match1.Groups[1].Value;
                string value = match1.Groups[2].Value;

                var match2 = _regex2.Match(value);

                if (match2.Success)
                {
                    string address = match2.Groups[1].Value;
                    int port = int.Parse(match2.Groups[2].Value);

                    var contexts = new List<InformationContext>();
                    contexts.Add(new InformationContext("Scheme", scheme));
                    contexts.Add(new InformationContext("Address", address));
                    contexts.Add(new InformationContext("Port", port));

                    return new Information(contexts);
                }
                else
                {
                    var contexts = new List<InformationContext>();
                    contexts.Add(new InformationContext("Scheme", scheme));
                    contexts.Add(new InformationContext("Address", value));

                    return new Information(contexts);
                }
            }

            return null;
        }
    }
}
