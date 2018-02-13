using System;
using System.Runtime.InteropServices;

namespace Windows
{
	public static class WindowsFunctions
	{
        [DllImport("user32.dll")]
        public static extern IntPtr GetClipboardViewer();

        [DllImport("user32.dll")]
		public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
		[DllImport("user32.dll")]
		public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);
	}
}
