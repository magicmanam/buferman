using BuferMAN.Clipboard;
using BuferMAN.Form.Properties;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BuferMAN.Form.Tests
{
    [TestClass]
    public class BuferHandlersWrapperTests
    {
        [TestMethod]
        public void On_Text_format_button_has_data_as_tag_and_text_trimmed()
        {
            var button = new Button();
            var buferService = A.Fake<IClipboardBuferService>();
            var originText = " Text";
            var data = new DataObject(DataFormats.Text, originText);
            data.SetData(DataFormats.StringFormat, string.Empty);
            var wrapper = new BuferHandlersWrapper(buferService, data, button, new System.Windows.Forms.Form(), A.Fake<IClipMenuGenerator>(), A.Fake<IBuferSelectionHandler>());

            Assert.AreEqual(originText, button.Tag);
            Assert.AreEqual(originText.Trim(), button.Text);
        }

        [TestMethod]
        public void On_String_format_button_has_data_as_tag_and_text_trimmed()
        {
            var button = new Button();
            var buferService = A.Fake<IClipboardBuferService>();
            var originText = "  String";
            var data = new DataObject(DataFormats.StringFormat, originText);
            data.SetData(DataFormats.Text, " Text");
            data.SetData(DataFormats.UnicodeText, null);
            var wrapper = new BuferHandlersWrapper(buferService, data, button, new System.Windows.Forms.Form(), A.Fake<IClipMenuGenerator>(), A.Fake<IBuferSelectionHandler>());

            Assert.AreEqual(originText, button.Tag);
            Assert.AreEqual(originText.Trim(), button.Text);
        }

        [TestMethod]
        public void On_Unicode_format_button_has_data_as_tag_and_text_trimmed()
        {
            var button = new Button();
            var buferService = A.Fake<IClipboardBuferService>();
            var originText = "   Unicode";
            var data = new DataObject(DataFormats.UnicodeText, originText);
            data.SetData(DataFormats.Text, " Text");
            data.SetData(DataFormats.StringFormat, " String");
            var wrapper = new BuferHandlersWrapper(buferService, data, button, new System.Windows.Forms.Form(), A.Fake<IClipMenuGenerator>(), A.Fake<IBuferSelectionHandler>());

            Assert.AreEqual(originText, button.Tag);
            Assert.AreEqual(originText.Trim(), button.Text);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_button_has_text_with_filename()
        {
            var button = new Button();
            var buferService = A.Fake<IClipboardBuferService>();
            var file = "c:\\";//TODO replace with IStorage interface
            var data = new DataObject(DataFormats.FileDrop, new string[] { file });
            var wrapper = new BuferHandlersWrapper(buferService, data, button, new System.Windows.Forms.Form(), A.Fake<IClipMenuGenerator>(), A.Fake<IBuferSelectionHandler>());

            Assert.AreEqual($"<< {Resource.FileBufer} >>", button.Text);
            Assert.AreEqual(" " + Environment.NewLine + Environment.NewLine + Path.DirectorySeparatorChar.ToString(), button.Tag);
        }

        [TestMethod]
        public void On_FileDrop_format_and_two_files_button_has_text_with_files_count()
        {
            var button = new Button();
            var buferService = A.Fake<IClipboardBuferService>();
            var file = "c:\\";//TODO replace with IStorage interface
            var data = new DataObject(DataFormats.FileDrop, new string[] { file, "d:\\" });
            var wrapper = new BuferHandlersWrapper(buferService, data, button, new System.Windows.Forms.Form(), A.Fake<IClipMenuGenerator>(), A.Fake<IBuferSelectionHandler>());

            Assert.AreEqual($"<< {Resource.FilesBufer} (2) >>", button.Text);
            //Assert.AreEqual(" " + Environment.NewLine + Environment.NewLine + Path.DirectorySeparatorChar.ToString(), button.Tag);
        }

        [TestMethod]
        public void On_CUSTOM_IMAGE_FORMAT_button_has_image_bufer_in_text()
        {
            var button = new Button();
            var buferService = A.Fake<IClipboardBuferService>();
            var oldFont = button.Font;
            var data = new DataObject(ClipboardFormats.CUSTOM_IMAGE_FORMAT, new object());
            var wrapper = new BuferHandlersWrapper(buferService, data, button, new System.Windows.Forms.Form(), A.Fake<IClipMenuGenerator>(), A.Fake<IBuferSelectionHandler>());

            Assert.AreEqual($"<< {Resource.ImageBufer} >>", button.Text);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), button.Font);
        }

        [TestMethod]
        public void On_null_button_has_text_with_whitespaces_count()
        {
            var button = new Button();
            var buferService = A.Fake<IClipboardBuferService>();
            var oldFont = button.Font;
            var data = new DataObject(DataFormats.UnicodeText, null);
            var wrapper = new BuferHandlersWrapper(buferService, data, button, new System.Windows.Forms.Form(), A.Fake<IClipMenuGenerator>(), A.Fake<IBuferSelectionHandler>());

            Assert.AreEqual($"<< {Resource.NotTextBufer} >>", button.Text);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), button.Font);
        }

        [TestMethod]
        public void On_whitespaces_button_has_text_with_whitespaces_count()
        {
            var button = new Button();
            var buferService = A.Fake<IClipboardBuferService>();
            var oldFont = button.Font;
            var data = new DataObject(DataFormats.UnicodeText, "   ");
            var wrapper = new BuferHandlersWrapper(buferService, data, button, new System.Windows.Forms.Form(), A.Fake<IClipMenuGenerator>(), A.Fake<IBuferSelectionHandler>());

            Assert.AreEqual($"<< 3   {Resource.WhiteSpaces} >>", button.Text);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), button.Font);
        }
    }
}