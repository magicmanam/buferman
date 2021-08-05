using BuferMAN.Clipboard;
using BuferMAN.Infrastructure;
using BuferMAN.Infrastructure.ContextMenu;
using BuferMAN.Infrastructure.Settings;
using BuferMAN.Infrastructure.Files;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BuferMAN.View;

namespace BuferMAN.WinForms.Tests
{
    [TestClass]
    public class BuferHandlersWrapperTests
    {
        private readonly Fake<IProgramSettingsGetter> _settings = new Fake<IProgramSettingsGetter>();

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
            this._settings.CallsTo(s => s.MaxBuferPresentationLength)
                .Returns(2000);

            // Act
            var wrapper = new BuferHandlersWrapper(
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                this._settings.FakedObject,
                bufer);

            // Assert
            Assert.AreEqual("Text", bufer.ViewModel.OriginBuferTitle);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_button_has_text_with_filename()
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
            var fileStorage = new Fake<IFileStorage>();
            fileStorage.CallsTo(s => s.GetFileAttributes(file)).Returns(FileAttributes.Normal);
            fileStorage.CallsTo(s => s.GetFileDirectory(file)).Returns("C:\\");
            fileStorage.CallsTo(s => s.GetFileName(file)).Returns("file.txt");

            // Act
            var wrapper = new BuferHandlersWrapper(
                data,
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                fileStorage.FakedObject,
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {file} >>", bufer.ViewModel.OriginBuferTitle);
            Assert.AreEqual("C:\\" + Environment.NewLine + Environment.NewLine + "file.txt", bufer.ViewModel.Representation);
        }

        [TestMethod]
        public void On_FileDrop_format_and_two_files_button_has_text_with_files_count()
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
            var fileStorage = new Fake<IFileStorage>();
            fileStorage.CallsTo(s => s.GetFileAttributes(file1)).Returns(FileAttributes.Normal);
            fileStorage.CallsTo(s => s.GetFileDirectory(file1)).Returns("c:\\");
            fileStorage.CallsTo(s => s.GetFileName(file1)).Returns("file1.ext");
            fileStorage.CallsTo(s => s.GetFileName(file2)).Returns("file2.ext");

            // Act
            var wrapper = new BuferHandlersWrapper(
                data,
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                fileStorage.FakedObject,
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {Resource.FilesBufer} (2) >>", bufer.ViewModel.OriginBuferTitle);
            Assert.AreEqual("c:\\" + Environment.NewLine + Environment.NewLine + "file1.ext" + Environment.NewLine + "file2.ext", bufer.ViewModel.Representation);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_with_directory_button_has_text_with_files_count_and_filename_and_folder()
        {
            // Arrange
            var file = "c:\\file1.ext";
            var folder = "c:\\folder";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file, folder });
            var bufer = new Bufer
            {
                ViewModel = new BuferViewModel
                {
                    Clip = data
                }
            };
            var fileStorage = new Fake<IFileStorage>();
            fileStorage.CallsTo(s => s.GetFileAttributes(file)).Returns(FileAttributes.Normal);
            fileStorage.CallsTo(s => s.GetFileDirectory(file)).Returns("c:\\");
            fileStorage.CallsTo(s => s.GetFileName(file)).Returns("file1.ext");
            fileStorage.CallsTo(s => s.GetFileName(folder)).Returns("folder");
            fileStorage.CallsTo(s => s.GetFileAttributes(folder)).Returns(FileAttributes.Directory);

            // Act
            var wrapper = new BuferHandlersWrapper(
                data,
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                fileStorage.FakedObject,
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {Resource.FilesBufer} (2) >>", bufer.ViewModel.OriginBuferTitle);
            Assert.AreEqual("c:\\" + Environment.NewLine + Environment.NewLine + "file1.ext" + Environment.NewLine + "folder\\", bufer.ViewModel.Representation);
        }

        [TestMethod]
        public void On_CUSTOM_IMAGE_FORMAT_button_has_image_bufer_in_text()
        {
            // Arrange
            var data = new DataObject(ClipboardFormats.CUSTOM_IMAGE_FORMAT, new object());
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
                data,
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {Resource.ImageBufer} >>", bufer.ViewModel.OriginBuferTitle);
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
                    Clip = data
                }
            };
            var oldFont = bufer.GetButton().Font;

            // Act
            var wrapper = new BuferHandlersWrapper(
                data,
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< {Resource.FileContentsBufer} >>", bufer.ViewModel.OriginBuferTitle);
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
                data,
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
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
                data,
                A.Fake<IBuferContextMenuGenerator>(),
                A.Fake<IBuferSelectionHandlerFactory>(),
                A.Fake<IFileStorage>(),
                A.Fake<IBufermanHost>(),
                A.Fake<IProgramSettingsGetter>(),
                bufer);

            // Assert
            Assert.AreEqual($"<< 3   {Resource.WhiteSpaces} >>", bufer.ViewModel.OriginBuferTitle);
            Assert.AreEqual(new Font(oldFont, FontStyle.Italic | FontStyle.Bold), bufer.GetButton().Font);
        }
    }
}