using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardBufer
{
	public class DataObjectComparer : IEqualityComparer<IDataObject>
    {
        private static readonly IList<string> _stringFormats = new List<string>() { "Text", "System.String", "Rich Text Format", "UnicodeText", "OEMText", "Locale" };//"VX Clipboard Descriptor Format", "CF_VSSTGPROJECTITEMS" };
        private static readonly IList<string> _arrayFormats = new List<string>() { "FileName", "FileNameW", "FileDrop" };

        public bool Equals(IDataObject x, IDataObject y)
        {
			if (x == null || y == null)
            {
                return false;
            }

            if (x == y)
            {
                return true;
            }

            var xFormats = x.GetFormats();
            var yFormats = y.GetFormats();

            //
            //if (ConfigurationManager.AppSettings["inspectNewFormats"] == "true")
            {
                var allFormats = _stringFormats.Union(_arrayFormats);
                foreach (var f in xFormats.Union(yFormats).Where(f => !allFormats.Contains(f)))
                {
                    if (x.GetData(f) as string != null)
                    {
                        var d = x.GetData(f) as string;
                        MessageBox.Show($"Unknown string format {f} with value {d}");
                        _stringFormats.Add(f);
                    }
                    if (x.GetData(f) as string[] != null)
                    {
                        var d = x.GetData(f) as string [];
                        MessageBox.Show($"Unknown array string format {f} with value {d}");
                        _arrayFormats.Add(f);
                    }
                }
            }
            //

            if (xFormats.Length != yFormats.Length)
            {
                return false;
            }

            bool equals = false;
            foreach (var stringFormat in _stringFormats)
            {
                var xContains = xFormats.Contains(stringFormat);
                var yContains = yFormats.Contains(stringFormat);

                if(xContains ^ yContains)
                {
                    return false;
                } else
                {
                    if (xContains)
                    {
                        var xValueString = x.GetData(stringFormat) as string;
                        var yValueString = y.GetData(stringFormat) as string;

                        if (xValueString != yValueString)
                        {
                            return false;
                        }
                        else
                        {
                            equals = true;
                        }
                    }
                }
            }
            if (equals)
            {
                return true;
            }

            foreach (var arrayFormat in _arrayFormats)
            {
                var xContains = xFormats.Contains(arrayFormat);
                var yContains = yFormats.Contains(arrayFormat);

                if (xContains ^ yContains)
                {
                    return false;
                } else
                {
                    if (xContains)
                    {
                        var xValueArray = x.GetData(arrayFormat) as string[];
                        var yValueArray = y.GetData(arrayFormat) as string[];

                        if (xValueArray.Length != yValueArray.Length)
                        {
                            return false;
                        }

                        for (var i = 0; i < xValueArray.Length; i++)
                        {
                            if (xValueArray[i] != yValueArray[i])
                            {
                                return false;
                            }
                        }

                        equals = true;
                    }
                }
            }

            if (equals)
            {
                return true;
            }//Such construction should not be replaced with return equals;

            return false;
        }

        public int GetHashCode(IDataObject obj)
        {
            return obj.GetHashCode();
        }
    }
}
