using BuferMAN.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace BuferMAN.Files
{
    internal class JsonFileFormatter : IBufersFileFormatter
    {
        public IEnumerable<BuferItem> Parse(string content)
        {
            if (content == string.Empty)
            {
                return Enumerable.Empty<BuferItem>();
            }
            else
            {
                return JArray.Parse(content).ToObject<IEnumerable<BuferItem>>();
            }
        }

        public string ToString(IEnumerable<BuferItem> bufers)
        {
            return JArray.FromObject(bufers).ToString();
        }
    }
}
