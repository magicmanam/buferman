using BuferMAN.Infrastructure.Storage;
using BuferMAN.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace BuferMAN.Files
{
    public class JsonFileParser : IBufersFileParser
    {
        public IEnumerable<BuferItem> Parse(StreamReader stream)
        {
            var serializer = new JsonSerializer();

            return JArray.Parse(stream.ReadToEnd()).ToObject<IEnumerable<BuferItem>>();
        }
    }
}
