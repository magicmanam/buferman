using System.Windows.Forms;

namespace ClipboardViewer.Window
{
    interface IRenderingHandler
    {
        void Render();

        void OnKeyDown(object sender, KeyEventArgs e);
    }
}