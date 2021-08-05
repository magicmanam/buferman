using BuferMAN.Clipboard;
using BuferMAN.Infrastructure.Settings;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;
using BuferMAN.View;

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
                A.Fake<IProgramSettingsGetter>());

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
                A.Fake<IProgramSettingsGetter>());

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
                A.Fake<IProgramSettingsGetter>());

            // Act
            sut.TryHandleDataObject(viewModel);

            // Assert
            Assert.AreEqual(originText, viewModel.TextData);
            Assert.AreEqual(originText, viewModel.TextRepresentation);
        }
    }
}