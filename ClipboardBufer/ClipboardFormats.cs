using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardBufer
{
    public static class ClipboardFormats
    {
        public static IList<string> StringFormats = new List<string>() { UNICODE_STRING_FORMAT, "Text", "Rich Text Format", "UnicodeText", "OEMText", "Locale", "HTML Format" };//"VX Clipboard Descriptor Format", "CF_VSSTGPROJECTITEMS" };

        public static IList<string> FileFormats = new List<string>() { FILE_FORMAT, "FileName", "FileNameW" };

        public const string CUSTOM_IMAGE_FORMAT = "Buferman.Image";
        public const string PASSWORD_FORMAT = "Buferman.Password";
        public const string UNICODE_STRING_FORMAT = "System.String";
        public const string TEXT_STRING_FORMAT = "Text";
        public const string FILE_FORMAT = "FileDrop";

    }
}
