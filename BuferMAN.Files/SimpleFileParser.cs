﻿using BuferMAN.Models;
using System.Collections.Generic;
using System.IO;

namespace BuferMAN.Files
{
    public class SimpleFileParser : IBufersFileParser
    {
        public IEnumerable<BuferItem> Parse(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                var bufer = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(bufer))
                {
                    yield return new BuferItem { Text = bufer, Pinned = false };
                }
            }
        }
    }
}
