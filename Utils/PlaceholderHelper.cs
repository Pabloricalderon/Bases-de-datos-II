using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bases_de_datos_II.Utils
{
    public static class PlaceholderHelper
    {
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        /// <summary>
        /// Placeholder nativo (cue banner). Requiere TextBox de una línea
        /// y Application.EnableVisualStyles() en Program.cs.
        /// </summary>
        public static void SetPlaceholder(this TextBox tb, string text, bool showEvenWhenFocused = false)
        {
            EventHandler apply = null;
            apply = (s, e) =>
            {
                if (tb.IsHandleCreated)
                {
                    SendMessage(tb.Handle, EM_SETCUEBANNER,
                        (IntPtr)(showEvenWhenFocused ? 1 : 0), text);
                }
            };
            if (tb.IsHandleCreated) apply(null, EventArgs.Empty);
            else tb.HandleCreated += apply;
        }

        /// <summary>
        /// Fallback administrado (sirve también para multilinea).
        /// </summary>
        public static void SetPlaceholderManaged(this TextBox tb, string text, Color? color = null)
        {
            var phColor = color ?? SystemColors.GrayText;
            var normalColor = SystemColors.WindowText;
            bool showing = false;

            Action show = () =>
            {
                if (string.IsNullOrEmpty(tb.Text) && tb.Enabled)
                {
                    showing = true;
                    tb.ForeColor = phColor;
                    tb.Text = text;
                }
            };

            tb.GotFocus += (s, e) =>
            {
                if (showing)
                {
                    showing = false;
                    tb.Text = "";
                    tb.ForeColor = normalColor;
                }
            };
            tb.LostFocus += (s, e) => { if (string.IsNullOrEmpty(tb.Text)) show(); };
            tb.TextChanged += (s, e) =>
            {
                if (!tb.Focused && string.IsNullOrEmpty(tb.Text)) show();
            };

            if (tb.IsHandleCreated) show();
            else tb.HandleCreated += (s, e) => show();
        }
    }
}
