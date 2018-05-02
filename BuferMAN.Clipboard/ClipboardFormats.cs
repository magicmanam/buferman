using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public static class ClipboardFormats
    {
        public static IList<string> StringFormats = new List<string>() { DataFormats.StringFormat, DataFormats.Text, DataFormats.Rtf, DataFormats.UnicodeText, DataFormats.OemText, DataFormats.Locale, DataFormats.Html };//"VX Clipboard Descriptor Format", "CF_VSSTGPROJECTITEMS" };

        public static IList<string> FileFormats = new List<string>() { DataFormats.FileDrop, "FileName", "FileNameW" };

        public const string CUSTOM_IMAGE_FORMAT = "Buferman.Image";
        public const string PASSWORD_FORMAT = "Buferman.Password";
        public const string SKYPE_FORMAT = "SkypeMessageFragment";
        public const string FTP_FILE_FORMAT = "FtpPrivateData";

    }
}
