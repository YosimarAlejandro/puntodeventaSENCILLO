using System;
using System.Linq;
using System.Windows.Forms;
using MiPOS.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MiPOS.UI;
using System.Drawing;

namespace MiPOS.Views.Sales
{
    public class SalesViewControl : UserControl
    {
        private TextBox txtMonto = null!;
        private Button btnAgregar = null!;
        private Button btnActualizar = null!;
        private Button btnBorrar = null!;
        private ListView lvSales = null!;
        private PosDbContext db = null!;

        private readonly string role;
        private readonly string username;

        public SalesViewControl(string role, string username)
        {
            this.role = role;
            this.username = username;
            InitializeComponent();
            db = new PosDbContext();
            LoadSales();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.PanelLight;

            txtMonto = new TextBox { Left = 10, Top = 10, Width = 400, Font = Theme.LargeInputFont, Anchor = AnchorStyles.Top | AnchorStyles.Left };
            Theme.SetPlaceholder(txtMonto, "0.00");
            Controls.Add(txtMonto);

            btnAgregar = new Button { Left = 420, Top = 10, Width = 120, Height = 50, Text = "Agregar" };
            Theme.StylePrimaryButton(btnAgregar);
            Theme.WithIcon(btnAgregar, "➕");
            btnAgregar.Click += BtnAgregar_Click;
            Controls.Add(btnAgregar);

            btnActualizar = new Button { Left = 550, Top = 10, Width = 120, Height = 50, Text = "Actualizar" };
            Theme.StyleSecondaryButton(btnActualizar);
            Theme.WithIcon(btnActualizar, "✏️", primary: false);
            btnActualizar.Click += BtnActualizar_Click;
            Controls.Add(btnActualizar);

            btnBorrar = new Button { Left = 680, Top = 10, Width = 120, Height = 50, Text = "Borrar" };
            Theme.StyleSecondaryButton(btnBorrar);
            Theme.WithIcon(btnBorrar, "🗑️", primary: false);
            btnBorrar.Click += BtnBorrar_Click;
            Controls.Add(btnBorrar);

            lvSales = new ListView { Left = 10, Top = 80, Width = 920, Height = 420, View = View.Details, FullRowSelect = true, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            lvSales.Columns.Add("Id", 60);
            lvSales.Columns.Add("Fecha", 260);
            lvSales.Columns.Add("Monto", 140);
            lvSales.Font = new Font("Segoe UI", 10);
            lvSales.SelectedIndexChanged += LvSales_SelectedIndexChanged;
            Controls.Add(lvSales);

            this.Resize += (s, e) =>
            {
                lvSales.Width = Math.Max(400, this.ClientSize.Width - 20);
                lvSales.Height = Math.Max(200, this.ClientSize.Height - 90);
            };
        }

        public void LoadSales()
        {
            if (lvSales == null) return;
            lvSales.Items.Clear();
            var list = db.Sales.OrderByDescending(s => s.Fecha).ToList();
            foreach (var s in list)
            {
                var row = new ListViewItem(s.Id.ToString());
                row.SubItems.Add(s.Fecha.ToString("yyyy-MM-dd HH:mm:ss"));
                row.SubItems.Add(s.Monto.ToString("F2", CultureInfo.InvariantCulture));
                row.Tag = s;
                lvSales.Items.Add(row);
            }
        }

        private void TxtMonto_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
                e.Handled = true;
            if ((e.KeyChar == '.' || e.KeyChar == ',') && (txtMonto.Text.Contains('.') || txtMonto.Text.Contains(',')))
                e.Handled = true;
        }

        private bool TryParseMonto(out decimal monto)
        {
            monto = 0;
            var txt = txtMonto.Text.Trim();
            var ph = txtMonto.Tag as string;
            if (!string.IsNullOrEmpty(ph) && txt == ph) { return false; }
            txt = txt.Replace(',', '.');
            return decimal.TryParse(txt, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out monto) && monto >= 0;
        }

        private void BtnAgregar_Click(object? sender, EventArgs e)
        {
            if (TryParseMonto(out decimal monto))
            {
                var sale = new Sale { Fecha = DateTime.Now, Monto = monto };
                db.Sales.Add(sale);
                db.SaveChanges();
                LoadSales();
                txtMonto.Text = "";
            }
            else MessageBox.Show("Monto inválido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnActualizar_Click(object? sender, EventArgs e)
        {
            if (lvSales.SelectedItems.Count == 0) { MessageBox.Show("Selecciona una venta para actualizar"); return; }
            if (!TryParseMonto(out decimal monto)) { MessageBox.Show("Monto inválido"); return; }

            var item = (Sale)lvSales.SelectedItems[0].Tag!;
            var sale = db.Sales.Find(item.Id);
            if (sale != null)
            {
                sale.Monto = monto;
                db.SaveChanges();
                LoadSales();
                txtMonto.Text = "";
            }
        }

        private void BtnBorrar_Click(object? sender, EventArgs e)
        {
            if (lvSales.SelectedItems.Count == 0) { MessageBox.Show("Selecciona una venta para borrar"); return; }
            var item = (Sale)lvSales.SelectedItems[0].Tag!;
            var sale = db.Sales.Find(item.Id);
            if (sale != null)
            {
                if (MessageBox.Show($"¿Borrar venta {sale.Id} - {sale.Monto:C}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    db.Sales.Remove(sale);
                    db.SaveChanges();
                    LoadSales();
                }
            }
        }

        private void LvSales_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lvSales.SelectedItems.Count == 0) return;
            var sale = (Sale)lvSales.SelectedItems[0].Tag!;
            txtMonto.Text = sale.Monto.ToString("F2", CultureInfo.InvariantCulture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
    }
}
