using BuferMAN.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BuferMAN.Files
{
    public class JsonFileParser : IBufersFileParser
    {
        public IEnumerable<BuferItem> Parse(StreamReader stream)
        {
            var serializer = new JsonSerializer();

            var content = stream.ReadToEnd();
            if (content == string.Empty)
            {
                return Enumerable.Empty<BuferItem>();
            }
            else
            {
                return JArray.Parse(content).ToObject<IEnumerable<BuferItem>>();
            }
        }
    }
}
