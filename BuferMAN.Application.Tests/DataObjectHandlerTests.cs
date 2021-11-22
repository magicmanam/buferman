using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Settings;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
using BuferMAN.View;
using BuferMAN.Infrastructure.Files;
using System;
using System.IO;

namespace BuferMAN.Application.Tests
{
    [TestClass]
    public class DataObjectHandlerTests
    {
        [TestMethod]
        public void On_Not_Empty_Text_Format_Only_Handles_TextData_And_TextRepresentation()
        {
            // Arrange
            var originText = " Text";
            var data = new DataObject(DataFormats.Text, originText);
            data.SetData(DataFormats.StringFormat, string.Empty);
            var viewModel = new BuferViewModel()
            {
                Clip = data
            };

            var sut = new DataObjectHandler(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IFileStorage>());

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.AreEqual(originText, viewModel.TextData);
            Assert.AreEqual(originText, viewModel.TextRepresentation);
        }

        [TestMethod]
        public void On_Not_Empty_String_Format_Handles_TextData_And_TextRepresentation()
        {
            // Arrange
            var originText = "  String";
            var data = new DataObject(DataFormats.StringFormat, originText);
            data.SetData(DataFormats.Text, " Text");
            data.SetData(DataFormats.UnicodeText, null);
            var viewModel = new BuferViewModel
            {
                Clip = data
            };

            var sut = new DataObjectHandler(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IFileStorage>());

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.AreEqual(originText, viewModel.TextData);
            Assert.AreEqual(originText, viewModel.TextRepresentation);
        }

        [TestMethod]
        public void On_Not_Empty_Unicode_Format_Handles_TextData_And_TextRepresentation()
        {
            // Arrange
            var originText = "   Unicode";
            var data = new DataObject(DataFormats.UnicodeText, originText);
            data.SetData(DataFormats.Text, " Text");
            data.SetData(DataFormats.StringFormat, " String");
            var viewModel = new BuferViewModel
            {
                Clip = data
            };

            var sut = new DataObjectHandler(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IFileStorage>());

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.AreEqual(originText, viewModel.TextData);
            Assert.AreEqual(originText, viewModel.TextRepresentation);
        }

        [TestMethod]
        public void On_CUSTOM_IMAGE_FORMAT_button_has_image_bufer_in_text()
        {
            // Arrange
            var data = new DataObject(ClipboardFormats.CUSTOM_IMAGE_FORMAT, new object());
            var viewModel = new BuferViewModel
            {
                Clip = data
            };

            var sut = new DataObjectHandler(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IFileStorage>());

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.IsFalse(viewModel.IsChangeTextAvailable);
            Assert.AreEqual(Resource.ImageBufer, viewModel.Representation);
            Assert.AreEqual(Resource.ImageBufer, viewModel.TextRepresentation);
        }

        [TestMethod]
        public void On_FILE_CONTENTS_FORMAT_button_has_file_contents_in_text()
        {
            // Arrange
            var data = new DataObject(ClipboardFormats.FILE_CONTENTS_FORMAT, new object());
            var viewModel = new BuferViewModel
            {
                Clip = data
            };

            var sut = new DataObjectHandler(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IProgramSettingsGetter>(),
                A.Fake<IFileStorage>());

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.IsFalse(viewModel.IsChangeTextAvailable);
            Assert.AreEqual(Resource.FileContentsBufer, viewModel.Representation);
            Assert.AreEqual(Resource.FileContentsBufer, viewModel.TextRepresentation);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_has_text_with_filename()
        {
            // Arrange
            var file = "c:\\file.txt";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file });
            var viewModel = new BuferViewModel
                {
                    Clip = data
                };
            var fileStorage = new Fake<IFileStorage>();
            fileStorage.CallsTo(s => s.GetFileAttributes(file)).Returns(FileAttributes.Normal);
            fileStorage.CallsTo(s => s.GetFileDirectory(file)).Returns("C:\\");
            fileStorage.CallsTo(s => s.GetFileName(file)).Returns("file.txt");
            var sut = new DataObjectHandler(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IProgramSettingsGetter>(),
                fileStorage.FakedObject);

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.AreEqual("C:\\" + Environment.NewLine + Environment.NewLine + "file.txt", viewModel.TextRepresentation);
        }

        [TestMethod]
        public void On_FileDrop_format_and_two_files_has_text_with_files_count()
        {
            // Arrange
            var file1 = "c:\\file1.ext";
            var file2 = "c:\\file2.ext";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file1, file2 });
            var viewModel = new BuferViewModel
                {
                    Clip = data
                };
            var fileStorage = new Fake<IFileStorage>();
            fileStorage.CallsTo(s => s.GetFileAttributes(file1)).Returns(FileAttributes.Normal);
            fileStorage.CallsTo(s => s.GetFileDirectory(file1)).Returns("c:\\");
            fileStorage.CallsTo(s => s.GetFileName(file1)).Returns("file1.ext");
            fileStorage.CallsTo(s => s.GetFileName(file2)).Returns("file2.ext");
            var sut = new DataObjectHandler(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IProgramSettingsGetter>(),
                fileStorage.FakedObject);

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.AreEqual("c:\\" + Environment.NewLine + Environment.NewLine + "file1.ext" + Environment.NewLine + "file2.ext", viewModel.Representation);
        }

        [TestMethod]
        public void On_FileDrop_format_and_one_file_with_directory_has_text_with_files_count_and_filename_and_folder()
        {
            // Arrange
            var file = "c:\\file1.ext";
            var folder = "c:\\folder";
            var data = new DataObject(DataFormats.FileDrop, new string[] { file, folder });
            var viewModel = new BuferViewModel
            {
                Clip = data
            };
            var fileStorage = new Fake<IFileStorage>();
            fileStorage.CallsTo(s => s.GetFileAttributes(file)).Returns(FileAttributes.Normal);
            fileStorage.CallsTo(s => s.GetFileDirectory(file)).Returns("c:\\");
            fileStorage.CallsTo(s => s.GetFileName(file)).Returns("file1.ext");
            fileStorage.CallsTo(s => s.GetFileName(folder)).Returns("folder");
            fileStorage.CallsTo(s => s.GetFileAttributes(folder)).Returns(FileAttributes.Directory);

            var sut = new DataObjectHandler(
                A.Fake<IClipboardBuferService>(),
                A.Fake<IProgramSettingsGetter>(),
                fileStorage.FakedObject);

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.AreEqual("c:\\" + Environment.NewLine + Environment.NewLine + "file1.ext" + Environment.NewLine + "folder\\", viewModel.Representation);
        }
    }
}