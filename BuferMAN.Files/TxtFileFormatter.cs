using BuferMAN.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BuferMAN.Files
{
    internal class TxtFileFormatter : IBufersFileFormatter
    {
        public IEnumerable<BuferItem> Parse(string content)
        {
            foreach (var line in content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                yield return new BuferItem { Text = line, Pinned = false };
            }
        }

        public string ToString(IEnumerable<BuferItem> buferItems)
        {
            var result = new StringBuilder();

            foreach (var buferItem in buferItems)
            {
                result.Append(buferItem.Text + "\r\n");
            }

            return result.ToString();
        }
    }
}
