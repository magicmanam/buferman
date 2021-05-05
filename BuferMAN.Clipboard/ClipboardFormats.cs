using System.Collections.Generic;
using System.Windows.Forms;

namespace BuferMAN.Clipboard
{
    public static class ClipboardFormats
    {
        public static IList<string> StringFormats = new List<string>() { DataFormats.StringFormat, DataFormats.Text, DataFormats.Rtf, DataFormats.UnicodeText, DataFormats.OemText, DataFormats.Locale, DataFormats.Html };// TODO (s) "VX Clipboard Descriptor Format";
        public static IList<string> TextFormats = new List<string>() { DataFormats.StringFormat, DataFormats.Text, DataFormats.UnicodeText };

        public static IList<string> FileFormats = new List<string>() { DataFormats.FileDrop, "FileName", "FileNameW" };

        public const string CUSTOM_IMAGE_FORMAT = "Buferman.Image";
        public const string PASSWORD_FORMAT = "Buferman.Password";
        public const string FROM_FILE_FORMAT = "Buferman.FromFile";

        public const string SKYPE_FORMAT = "SkypeMessageFragment";
        public const string FTP_FILE_FORMAT = "FtpPrivateData";
        public const string FILE_CONTENTS_FORMAT = "FileContents";
        public const string VISUAL_STUDIO_PROJECT_ITEMS = "CF_VSSTGProjectitems";

    }
}
