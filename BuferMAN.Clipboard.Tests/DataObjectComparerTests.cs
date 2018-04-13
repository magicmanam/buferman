using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Forms;

namespace BuferMAN.Clipboard.Tests
{
    [TestClass]
    public class DataObjectComparerTests
    {
        private DataObjectComparer _comparer;

        [TestInitialize]
        public void On_Init()
        {
            this._comparer = new DataObjectComparer(ClipboardFormats.StringFormats, ClipboardFormats.FileFormats);
        }

        [TestMethod]
        public void On_null_argument_Equals_returns_False()
        {
            var result1 = this._comparer.Equals(null, new DataObject());
            var result2 = this._comparer.Equals(new DataObject(), null);

            Assert.IsFalse(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void On_the_same_objects_Equals_returns_True()
        {
            var dataObject = new DataObject("format", new object());

            var result = this._comparer.Equals(dataObject, dataObject);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void On_different_format_length_Equals_returns_False()
        {
            var obj1 = new DataObject("format", new object());
            obj1.SetText("text");
            var obj2 = new DataObject("format", new object());

            var result = this._comparer.Equals(obj1, obj2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void On_different_text_for_same_string_format_Equals_returns_False()
        {
            var obj1 = new DataObject(ClipboardFormats.StringFormats[0], "str1");
            var obj2 = new DataObject(ClipboardFormats.StringFormats[0], "str2");

            var result = this._comparer.Equals(obj1, obj2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void On_same_text_formats_Equals_returns_True()
        {
            var obj1 = new DataObject(ClipboardFormats.StringFormats[0], "str1");
            obj1.SetData("f1", new object());
            var obj2 = new DataObject(ClipboardFormats.StringFormats[0], "str1");
            obj2.SetData("f2", new object());

            var result = this._comparer.Equals(obj1, obj2);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void On_different_file_list_length_for_same_file_format_Equals_returns_False()
        {
            var obj1 = new DataObject(ClipboardFormats.FileFormats[0], new string[] { "str1" });
            var obj2 = new DataObject(ClipboardFormats.FileFormats[0], new string[] { "str1", "str2" });

            var result = this._comparer.Equals(obj1, obj2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void On_same_file_formats_Equals_returns_True()
        {
            var obj1 = new DataObject(ClipboardFormats.FileFormats[0], new string[] { "str1", "str2" });
            obj1.SetData("f1", new object());
            var obj2 = new DataObject(ClipboardFormats.FileFormats[0], new string[] { "str1", "str2" });
            obj2.SetData("f2", new object());

            var result = this._comparer.Equals(obj1, obj2);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void On_diffrerent_file_lists_for_same_file_format_Equals_returns_False()
        {
            var obj1 = new DataObject(ClipboardFormats.FileFormats[0], new string[] { "str1", "str2" });
            obj1.SetData(ClipboardFormats.FileFormats[1], new string[] { "str1", "str2" });
            var obj2 = new DataObject(ClipboardFormats.FileFormats[0], new string[] { "str1", "str2" });
            obj2.SetData(ClipboardFormats.FileFormats[1], new string[] { "str1", "str3" });

            var result = this._comparer.Equals(obj1, obj2);

            Assert.IsFalse(result);
        }
    }
}
