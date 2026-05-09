using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using stash.Models;

namespace stash.Services
{
    public class ClipboardService : IDisposable
    {
        // Tell C# we want to use this function from Windows itself
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hwnd);

        // Windows sends this message code whenever clipboard changes
        private const int WM_CLIPBOARDUPDATE = 0x031D;

        private HwndSource? _hwndSource;

        // This event fires every time something is copied
        public event Action<ClipItem>? ClipboardChanged;

        public void StartListening(Window window)
        {
            var helper = new WindowInteropHelper(window);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _hwndSource.AddHook(WndProc);
            AddClipboardFormatListener(helper.Handle);
        }

        // Windows calls this every time a message is sent to our window
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam,IntPtr lParam, ref bool handled)
        {
            if (msg == WM_CLIPBOARDUPDATE)
            {
                var item = ReadClipboard();
                if (item != null)
                    ClipboardChanged?.Invoke(item);
            }
            return IntPtr.Zero;
        }

        private ClipItem? ReadClipboard()
        {
            try
            {
                // Image copied
                if (Clipboard.ContainsImage())
                {
                    var bmp = Clipboard.GetImage();
                    if (bmp == null) return null;
                    return new ClipItem
                    {
                        Type = ClipType.Image,
                        ImageData = BitmapSourceToBytes(bmp)
                    };
                }

                // Text copied
                if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();
                    if (string.IsNullOrWhiteSpace(text)) return null;

                    var type = DetectType(text);
                    return new ClipItem
                    {
                        Type = type,
                        Content = text
                    };
                }
            }
            catch { }

            return null;
        }

        private ClipType DetectType(string text)
        {
            if (Uri.TryCreate(text.Trim(), UriKind.Absolute, out _))
                return ClipType.Url;

            if (text.Contains('\n') &&
               (text.Contains('{') || text.Contains(';') || text.Contains("=>")))
                return ClipType.Code;

            return ClipType.Text;
        }

        private byte[] BitmapSourceToBytes(System.Windows.Media.Imaging.BitmapSource bmp)
        {
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(
                System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
            using var ms = new System.IO.MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }

        public void Dispose()
        {
            if (_hwndSource != null)
            {
                RemoveClipboardFormatListener(_hwndSource.Handle);
                _hwndSource.RemoveHook(WndProc);
            }
        }
    }
}