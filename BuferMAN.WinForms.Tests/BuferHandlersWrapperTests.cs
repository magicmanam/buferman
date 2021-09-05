using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Files;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BuferMAN.View;
using BuferMAN.Infrastructure.Plugins;
using System.Collections.Generic;

namespace BuferMAN.WinForms.Tests
{
    [TestClass]
    public class BuferHandlersWrapperTests
    {
        private readonly Fake<IProgramSettingsGetter> _settingsGetter = new Fake<IProgramSettingsGetter>();
        private readonly Fake<IProgramSettingsSetter> _settingsSetter = new Fake<IProgramSettingsSetter>();

        [TestMethod]
        public void Sets_Trimmed_TextRepresentation_To_OriginBuferTitle()
        {
            // Arrange
            var originText = " Text";
            var data = new DataObject(DataFormats.Text, originText);
            data.SetData(DataFormats.StringFormat, string.Empty);
            var bufer = new Bufer()
            {
                ViewModel = new BuferViewModel()
                {
                    Clip = data,
                    TextRepresentation = originText
                }
            };

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                this._settingsGetter.FakedObject,
                A.Fake<IProgramSettingsSetter>(),
                new List<IBufermanPlugin>(),
                bufer);

            // Assert
            Assert.AreEqual("Text", bufer.ViewModel.OriginBuferTitle);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_button_has_title_with_filename()
        {
            // Arrange
            var file = "c:\\file.txt";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file });
            var bufer = new Bufer
            {
                ViewModel = new BuferViewModel
                {
                    Clip = data
                }
            };

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IProgramSettingsSetter>(),
                new List<IBufermanPlugin>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {file} >>", bufer.ViewModel.OriginBuferTitle);
        }

        [TestMethod]
        public void On_FileDrop_format_and_two_files_button_has_title_as_files_bufer()
        {
            // Arrange
            var file1 = "c:\\file1.ext";
            var file2 = "c:\\file2.ext";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file1, file2 });
            var bufer = new Bufer
            {
                ViewModel = new BuferViewModel
                {
                    Clip = data
                }
            };

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IProgramSettingsSetter>(),
                new List<IBufermanPlugin>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {Resource.FilesBufer} (2) >>", bufer.ViewModel.OriginBuferTitle);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_with_directory_button_has_title_as_files_bufer()
        {
            // Arrange
            var file = "c:\\file1.ext";
            var folder = "c:\\folder";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file, folder });
            var bufer = new Bufer
            {
                ViewModel = new BuferViewModel
                {
                    Clip = data,
                    TextRepresentation = "Files bufer"
                }
            };

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IProgramSettingsSetter>(),
                new List<IBufermanPlugin>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {Resource.FilesBufer} (2) >>", bufer.ViewModel.OriginBuferTitle);
        }

        [TestMethod]
        public void On_CUSTOM_IMAGE_FORMAT_button_has_italic_and_bold_text()
        {
            // Arrange
            var data = new DataObject(ClipboardFormats.CUSTOM_IMAGE_FORMAT, new object());
            var bufer = new Bufer
            {
                ViewModel = new BuferViewModel
                {
                    Clip = data,
                    TextRepresentation = "Image"
                }
            };
            var oldFont = bufer.GetButton().Font;

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IProgramSettingsSetter>(),
                new List<IBufermanPlugin>(),
                bufer);

            // Assert
            Assert.AreEqual("Image", bufer.ViewModel.OriginBuferTitle);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), bufer.GetButton().Font);
        }

        [TestMethod]
        public void On_FILE_CONTENTS_FORMAT_button_has_file_contents_in_text()
        {
            // Arrange
            var data = new DataObject(ClipboardFormats.FILE_CONTENTS_FORMAT, new object());
            var bufer = new Bufer
            {
                ViewModel = new BuferViewModel
                {
                    Clip = data,
                    TextRepresentation = "File Content"
                }
            };
            var oldFont = bufer.GetButton().Font;

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IProgramSettingsSetter>(),
                new List<IBufermanPlugin>(),
                bufer);

            // Assert
            Assert.AreEqual("File Content", bufer.ViewModel.OriginBuferTitle);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), bufer.GetButton().Font);
        }

        [TestMethod]
        public void On_null_button_has_text_with_whitespaces_count()
        {
            // Arrange
            var data = new DataObject(DataFormats.UnicodeText, null);
            var bufer = new Bufer
            {
                ViewModel = new BuferViewModel
                {
                    Clip = data
                }
            };
            var oldFont = bufer.GetButton().Font;

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IProgramSettingsSetter>(),
                new List<IBufermanPlugin>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {Resource.NotTextBufer} >>", bufer.ViewModel.OriginBuferTitle);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), bufer.GetButton().Font);
        }

        [TestMethod]
        public void Sets_OriginalBuferTitle_With_Whitespaces_Count_On_whitespaces_clip()
        {
            // Arrange
            var data = new DataObject(DataFormats.UnicodeText, "   ");
            var bufer = new Bufer
            {
                ViewModel = new BuferViewModel
                {
                    Clip = data,
                    TextRepresentation = "   "
                }
            };
            var oldFont = bufer.GetButton().Font;

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IProgramSettingsSetter>(),
                new List<IBufermanPlugin>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< 3   {Resource.WhiteSpaces} >>", bufer.ViewModel.OriginBuferTitle);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), bufer.GetButton().Font);
        }
    }
}