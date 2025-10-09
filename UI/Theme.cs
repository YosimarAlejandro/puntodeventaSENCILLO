using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MiPOS.UI
{
    public static class Theme
    {
        // Paleta
        public static readonly Color BackgroundDark = Color.FromArgb(20, 24, 34);
        public static readonly Color Sidebar = Color.FromArgb(32, 36, 45);
        public static readonly Color PanelLight = Color.FromArgb(245, 246, 249);
        public static readonly Color Accent = Color.FromArgb(0, 120, 215); // azul Windows
        public static readonly Color AccentDark = Color.FromArgb(0, 100, 180);
        public static readonly Color NeutralButton = Color.FromArgb(80, 85, 92);
        public static readonly Font UiFont = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font UiFontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font LargeInputFont = new Font("Segoe UI", 20F, FontStyle.Regular);

        // Aplica estilo global al formulario
        public static void Apply(Form frm)
        {
            frm.BackColor = BackgroundDark;
            frm.Font = UiFont;
            ApplyToMenuStripControls(frm.Controls);
            // Recolor top-level MenuStrip if exists
            foreach (var m in frm.Controls.OfType<MenuStrip>()) StyleMenuStrip(m);
            // Style immediate child panels (sidebar, mainArea) if exist
        }

        // Estila recursivamente controles por tipo
        public static void StyleControl(Control c)
        {
            if (c is Panel) c.BackColor = PanelLight;
            if (c is Label l)
            {
                l.ForeColor = Color.FromArgb(30, 30, 30);
                l.Font = UiFontBold;
            }
            if (c is Button b)
            {
                StylePrimaryButton(b);
            }
            if (c is TextBox tb)
            {
                tb.Font = UiFont;
                tb.BackColor = Color.White;
                tb.ForeColor = Color.FromArgb(30, 30, 30);
            }
            if (c is ListView lv)
            {
                lv.BackColor = Color.White;
                lv.ForeColor = Color.FromArgb(30, 30, 30);
            }
            // recurse
            foreach (Control child in c.Controls)
                StyleControl(child);
        }

        // Aplica a todos los botones un estilo por defecto
        public static void StylePrimaryButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = Accent;
            b.ForeColor = Color.White;
            b.Font = UiFontBold;
            b.Padding = new Padding(8);
            b.Cursor = Cursors.Hand;
            // Hover
            b.MouseEnter += (s, e) => { b.BackColor = AccentDark; };
            b.MouseLeave += (s, e) => { b.BackColor = Accent; };
        }

        public static void StyleSecondaryButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.BackColor = NeutralButton;
            b.ForeColor = Color.White;
            b.Font = UiFontBold;
            b.Padding = new Padding(8);
            b.Cursor = Cursors.Hand;
            b.MouseEnter += (s, e) => { b.BackColor = ControlPaint.Light(NeutralButton); };
            b.MouseLeave += (s, e) => { b.BackColor = NeutralButton; };
        }

        public static void StyleSidebar(Panel sidebar)
        {
            sidebar.BackColor = Sidebar;
            foreach (Control c in sidebar.Controls)
            {
                if (c is Button b)
                {
                    b.ForeColor = Color.White;
                    b.BackColor = Color.FromArgb(63, 63, 70);
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 0;
                    b.Font = UiFontBold;
                }
                if (c is Label l)
                {
                    l.ForeColor = Color.White;
                    l.Font = UiFont;
                }
            }
        }

        public static void StyleMenuStrip(MenuStrip menu)
        {
            menu.RenderMode = ToolStripRenderMode.System;
            menu.BackColor = Sidebar;
            menu.ForeColor = Color.White;
            menu.Font = UiFontBold;
            menu.Padding = new Padding(6);
            foreach (ToolStripMenuItem item in menu.Items)
            {
                item.ForeColor = Color.White;
                item.Font = UiFontBold;
            }
        }

        // Helper para aplicar a todos los MenuStrip encontrados
        private static void ApplyToMenuStripControls(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
                if (c is MenuStrip ms) StyleMenuStrip(ms);
                else ApplyToMenuStripControls(c.Controls);
        }

        // Añade "icono" (emoji) al inicio del texto (rápido, sin imágenes)
        public static void WithIcon(Button btn, string emoji, bool primary = true)
        {
            if (primary) btn.Text = $"{emoji}  {btn.Text}";
            else btn.Text = $"{emoji}  {btn.Text}";
        }

        // Placeholder pequeño para TextBox (simple)
        public static void SetPlaceholder(TextBox tb, string placeholder)
        {
            tb.Tag = placeholder;
            tb.ForeColor = Color.Gray;
            tb.Text = placeholder;
            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.Black;
                }
            };
            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
                }
            };
        }
    }
}
