using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ClipboardBufer
{
	public class DataObjectComparer : IEqualityComparer<IDataObject>
    {
        private readonly IList<string> _stringFormats;
        private readonly IList<string> _arrayFormats;

        public DataObjectComparer(IList<string> stringFormats, IList<string> arrayFormats)
        {
            this._stringFormats = stringFormats;
            this._arrayFormats = arrayFormats;
        }

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
                foreach (var f in xFormats.Where(f => !allFormats.Contains(f)))
                {
                    if (x.GetData(f) as string != null)
                    {
                        this.TrackUnknownFormat("string", f);
                        _stringFormats.Add(f);
                    }
                    if (x.GetData(f) as string[] != null)
                    {
                        this.TrackUnknownFormat("array", f);
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

        private void TrackUnknownFormat(string type, string format)
        {
            using (var fs = new StreamWriter(new FileStream("unknown-formats.txt", FileMode.Append)))
            {
                fs.WriteLine(type);
                fs.WriteLine(format);
            }
        }
    }
}
