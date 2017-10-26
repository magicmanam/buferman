using System.Windows.Forms;

namespace ClipboardViewer
{
    interface IRenderingHandler
    {
        void Render();

        void OnKeyDown(object sender, KeyEventArgs e);
    }
}