using System;
using System.Linq;
using System.Windows.Forms;
using MiPOS.Models;
using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace MiPOS
{
    public class MainForm : Form
    {
        TextBox txtMonto;
        Button btnAgregar, btnActualizar, btnBorrar, btnRespaldar;
        ListView lvSales;
        PosDbContext db;

        public MainForm()
        {
            Text = "MiPOS - Punto de Venta (Una caja)";
            Width = 700;
            Height = 480;

            db = new PosDbContext();

            // Monto
            txtMonto = new TextBox { Left = 20, Top = 20, Width = 300, Font = new System.Drawing.Font("Segoe UI", 18) };
            txtMonto.KeyPress += TxtMonto_KeyPress; // validar solo números
            Controls.Add(txtMonto);

            // Agregar
            btnAgregar = new Button { Left = 340, Top = 20, Width = 100, Text = "Agregar" };
            btnAgregar.Click += BtnAgregar_Click;
            Controls.Add(btnAgregar);

            // Actualizar
            btnActualizar = new Button { Left = 450, Top = 20, Width = 100, Text = "Actualizar" };
            btnActualizar.Click += BtnActualizar_Click;
            Controls.Add(btnActualizar);

            // Borrar
            btnBorrar = new Button { Left = 560, Top = 20, Width = 100, Text = "Borrar" };
            btnBorrar.Click += BtnBorrar_Click;
            Controls.Add(btnBorrar);

            // ListView
            lvSales = new ListView { Left = 20, Top = 80, Width = 640, Height = 320, View = View.Details, FullRowSelect = true };
            lvSales.Columns.Add("Id", 60);
            lvSales.Columns.Add("Fecha", 260);
            lvSales.Columns.Add("Monto", 120);
            lvSales.SelectedIndexChanged += LvSales_SelectedIndexChanged;
            Controls.Add(lvSales);

            // Respaldar mes
            btnRespaldar = new Button { Left = 20, Top = 410, Width = 200, Text = "Respaldar mes (CSV + copia BD)" };
            btnRespaldar.Click += BtnRespaldar_Click;
            Controls.Add(btnRespaldar);

            LoadSales();
        }

        private void TxtMonto_KeyPress(object? sender, KeyPressEventArgs e)
        {
            // Permitir dígitos, backspace y punto/coma
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
                e.Handled = true;
            // Solo permitir una coma/punto
            if ((e.KeyChar == '.' || e.KeyChar == ',') && (txtMonto.Text.Contains('.') || txtMonto.Text.Contains(',')))
                e.Handled = true;
        }

        private void LoadSales()
        {
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

        private void BtnAgregar_Click(object? sender, EventArgs e)
        {
            if (TryParseMonto(out decimal monto))
            {
                var sale = new Sale { Fecha = DateTime.Now, Monto = monto };
                db.Sales.Add(sale);
                db.SaveChanges();
                LoadSales();
                txtMonto.Clear();
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
                txtMonto.Clear();
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

        private bool TryParseMonto(out decimal monto)
        {
            monto = 0;
            var txt = txtMonto.Text.Trim().Replace(',', '.');
            return decimal.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out monto) && monto >= 0;
        }

        private void BtnRespaldar_Click(object? sender, EventArgs e)
        {
            try
            {
                // Carpeta backups
                var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MiPOS");
                var backupsDir = Path.Combine(baseDir, "backups");
                Directory.CreateDirectory(backupsDir);

                // 1) Generar CSV con totales por mes
                var csvPath = Path.Combine(backupsDir, $"totales_por_mes_{DateTime.Now:yyyyMM_ddHHmmss}.csv");

                var totals = db.Sales
                    .AsEnumerable()
                    .GroupBy(s => new { s.Fecha.Year, s.Fecha.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(x => x.Monto) })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .ToList();

                var sb = new StringBuilder();
                sb.AppendLine("Año,Mes,Total");
                foreach (var t in totals)
                    sb.AppendLine($"{t.Year},{t.Month},{t.Total:F2}");

                File.WriteAllText(csvPath, sb.ToString(), Encoding.UTF8);

                // 2) Copia del archivo de BD (backup físico)
                var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MiPOS", "data");
                var dbPath = Path.Combine(dataFolder, "pos.db");
                if (File.Exists(dbPath))
                {
                    var dbBackup = Path.Combine(backupsDir, $"pos_db_backup_{DateTime.Now:yyyyMM_ddHHmmss}.db");
                    File.Copy(dbPath, dbBackup, true);
                }

                MessageBox.Show($"Respaldos generados:\nCSV: {csvPath}\n(Se copió pos.db a backups)", "Respaldo OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar respaldo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            db?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
