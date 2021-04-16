using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Storage;
using BuferMAN.View;
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
            var originText = " Text";
            var data = new DataObject(DataFormats.Text, originText);
            data.SetData(DataFormats.StringFormat, string.Empty);
            var wrapper = new BuferHandlersWrapper(new BuferViewModel { Clip = data }, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), A.Fake<IFileStorage>(), A.Fake<IBufermanHost>());

            Assert.AreEqual(originText, (button.Tag as BuferViewModel).Representation);
            Assert.AreEqual(originText.Trim(), button.Text);
        }

        [TestMethod]
        public void On_String_format_button_has_data_as_tag_and_text_trimmed()
        {
            var button = new Button();
            var originText = "  String";
            var data = new DataObject(DataFormats.StringFormat, originText);
            data.SetData(DataFormats.Text, " Text");
            data.SetData(DataFormats.UnicodeText, null);
            var wrapper = new BuferHandlersWrapper(new BuferViewModel { Clip = data }, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), A.Fake<IFileStorage>(), A.Fake<IBufermanHost>());

            Assert.AreEqual(originText, (button.Tag as BuferViewModel).Representation);
            Assert.AreEqual(originText.Trim(), button.Text);
        }

        [TestMethod]
        public void On_Unicode_format_button_has_data_as_tag_and_text_trimmed()
        {
            var button = new Button();
            var originText = "   Unicode";
            var data = new DataObject(DataFormats.UnicodeText, originText);
            data.SetData(DataFormats.Text, " Text");
            data.SetData(DataFormats.StringFormat, " String");
            var wrapper = new BuferHandlersWrapper(data, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), A.Fake<IFileStorage>(), A.Fake<IBufermanHost>());

            Assert.AreEqual(originText, (button.Tag as BuferViewModel).Representation);
            Assert.AreEqual(originText.Trim(), button.Text);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_button_has_text_with_filename()
        {
            var button = new Button();
            var file = "c:\\file.txt";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file });
            var fileStorage = A.Fake<IFileStorage>();
            A.CallTo(() => fileStorage.GetFileAttributes(file)).Returns(FileAttributes.Normal);
            A.CallTo(() => fileStorage.GetFileDirectory(file)).Returns("C:\\");
            A.CallTo(() => fileStorage.GetFileName(file)).Returns("file.txt");
            var wrapper = new BuferHandlersWrapper(data, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), fileStorage, A.Fake<IBufermanHost>());

            Assert.AreEqual($"<< {Resource.FileBufer} >>", button.Text);
            Assert.AreEqual("C:\\" + Environment.NewLine + Environment.NewLine + "file.txt", (button.Tag as BuferViewModel).Representation);
        }

        [TestMethod]
        public void On_FileDrop_format_and_two_files_button_has_text_with_files_count()
        {
            var button = new Button();
            var file1 = "c:\\file1.ext";
            var file2 = "c:\\file2.ext";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file1, file2 });
            var fileStorage = A.Fake<IFileStorage>();
            A.CallTo(() => fileStorage.GetFileAttributes(file1)).Returns(FileAttributes.Normal);
            A.CallTo(() => fileStorage.GetFileDirectory(file1)).Returns("c:\\");
            A.CallTo(() => fileStorage.GetFileName(file1)).Returns("file1.ext");
            A.CallTo(() => fileStorage.GetFileName(file2)).Returns("file2.ext");

            var wrapper = new BuferHandlersWrapper(data, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), fileStorage, A.Fake<IBufermanHost>());

            Assert.AreEqual($"<< {Resource.FilesBufer} (2) >>", button.Text);
            Assert.AreEqual("c:\\" + Environment.NewLine + Environment.NewLine + "file1.ext" + Environment.NewLine + "file2.ext", (button.Tag as BuferViewModel).Representation);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_with_directory_button_has_text_with_files_count_and_filename_and_folder()
        {
            var button = new Button();
            var file = "c:\\file1.ext";
            var folder = "c:\\folder";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file, folder });
            var fileStorage = A.Fake<IFileStorage>();
            A.CallTo(() => fileStorage.GetFileAttributes(file)).Returns(FileAttributes.Normal);
            A.CallTo(() => fileStorage.GetFileDirectory(file)).Returns("c:\\");
            A.CallTo(() => fileStorage.GetFileName(file)).Returns("file1.ext");
            A.CallTo(() => fileStorage.GetFileName(folder)).Returns("folder");
            A.CallTo(() => fileStorage.GetFileAttributes(folder)).Returns(FileAttributes.Directory);

            var wrapper = new BuferHandlersWrapper(data, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), fileStorage, A.Fake<IBufermanHost>());

            Assert.AreEqual($"<< {Resource.FilesBufer} (2) >>", button.Text);
            Assert.AreEqual("c:\\" + Environment.NewLine + Environment.NewLine + "file1.ext" + Environment.NewLine + "folder\\", (button.Tag as BuferViewModel).Representation);
        }

        [TestMethod]
        public void On_CUSTOM_IMAGE_FORMAT_button_has_image_bufer_in_text()
        {
            var button = new Button();
            var oldFont = button.Font;
            var data = new DataObject(ClipboardFormats.CUSTOM_IMAGE_FORMAT, new object());
            var wrapper = new BuferHandlersWrapper(data, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), A.Fake<IFileStorage>(), A.Fake<IBufermanHost>());

            Assert.AreEqual($"<< {Resource.ImageBufer} >>", button.Text);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), button.Font);
        }

        [TestMethod]
        public void On_FILE_CONTENTS_FORMAT_button_has_file_contents_in_text()
        {
            var button = new Button();
            var oldFont = button.Font;
            var data = new DataObject(ClipboardFormats.FILE_CONTENTS_FORMAT, new object());
            var wrapper = new BuferHandlersWrapper(data, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), A.Fake<IFileStorage>(), A.Fake<IBufermanHost>());

            Assert.AreEqual($"<< {Resource.FileContentsBufer} >>", button.Text);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), button.Font);
        }

        [TestMethod]
        public void On_null_button_has_text_with_whitespaces_count()
        {
            var button = new Button();
            var oldFont = button.Font;
            var data = new DataObject(DataFormats.UnicodeText, null);
            var wrapper = new BuferHandlersWrapper(data, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), A.Fake<IFileStorage>(), A.Fake<IBufermanHost>());

            Assert.AreEqual($"<< {Resource.NotTextBufer} >>", button.Text);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), button.Font);
        }

        [TestMethod]
        public void On_whitespaces_button_has_text_with_whitespaces_count()
        {
            var button = new Button();
            var oldFont = button.Font;
            var data = new DataObject(DataFormats.UnicodeText, "   ");
            var wrapper = new BuferHandlersWrapper(data, button, A.Fake<IBuferContextMenuGenerator>(), A.Fake<IBuferSelectionHandlerFactory>(), A.Fake<IFileStorage>(), A.Fake<IBufermanHost>());

            Assert.AreEqual($"<< 3   {Resource.WhiteSpaces} >>", button.Text);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), button.Font);
        }
    }
}