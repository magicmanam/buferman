using BuferMAN.Clipboard;
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

        public string ToString(IEnumerable<BuferItem> buferItems)
        {
            buferItems = buferItems.ToList();

            foreach (var buferItem in buferItems)
            {
                var formats = buferItem.Formats.Keys;

                foreach (var format in formats.ToList())
                {
                    if (!ClipboardFormats.StringFormats.Contains(format))
                    {
                        buferItem.Formats.Remove(format);
                    }
                }
            }

            return JArray.FromObject(buferItems).ToString();
        }
    }
}
