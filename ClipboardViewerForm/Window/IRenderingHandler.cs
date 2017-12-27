using System.Windows.Forms;

namespace ClipboardViewerForm.Window
{
    interface IRenderingHandler
    {
        void Render();

        void OnKeyDown(object sender, KeyEventArgs e);
    }
}