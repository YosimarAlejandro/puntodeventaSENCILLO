using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MiPOS.Models;
using MiPOS.UI;
using System.Drawing;

namespace MiPOS.Views.Admin
{
    public class AdminViewControl : UserControl
    {
        private PosDbContext db = null!;
        private Button btnGenCsv = null!;
        private Button btnBackupDb = null!;
        private TextBox txtOutput = null!;

        // Productos UI
        private DataGridView dgvProducts = null!;
        private Button btnAddProduct = null!;
        private Button btnEditProduct = null!;
        private Button btnDeleteProduct = null!;

        public AdminViewControl()
        {
            InitializeComponent();
            db = new PosDbContext();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.PanelLight;

            // Reportes / Backups
            btnGenCsv = new Button { Left = 10, Top = 10, Width = 250, Height = 50, Text = "Generar reportes (CSV)" };
            Theme.StylePrimaryButton(btnGenCsv);
            Theme.WithIcon(btnGenCsv, "📊");
            btnGenCsv.Click += (s, e) => GenerateCsvTotals();
            Controls.Add(btnGenCsv);

            btnBackupDb = new Button { Left = 280, Top = 10, Width = 250, Height = 50, Text = "Crear respaldo BD" };
            Theme.StyleSecondaryButton(btnBackupDb);
            Theme.WithIcon(btnBackupDb, "💾", primary: false);
            btnBackupDb.Click += (s, e) => BackupDb();
            Controls.Add(btnBackupDb);

            // Productos - botones CRUD
            btnAddProduct = new Button { Left = 10, Top = 70, Width = 160, Height = 52, Text = "Agregar producto" };
            Theme.StylePrimaryButton(btnAddProduct);
            Theme.WithIcon(btnAddProduct, "➕");
            btnAddProduct.Click += BtnAddProduct_Click;
            Controls.Add(btnAddProduct);

            btnEditProduct = new Button { Left = 180, Top = 70, Width = 160, Height = 52, Text = "Editar producto" };
            Theme.StyleSecondaryButton(btnEditProduct);
            Theme.WithIcon(btnEditProduct, "✏️", primary: false);
            btnEditProduct.Click += BtnEditProduct_Click;
            Controls.Add(btnEditProduct);

            btnDeleteProduct = new Button { Left = 350, Top = 70, Width = 160, Height = 52, Text = "Eliminar producto" };
            Theme.StyleSecondaryButton(btnDeleteProduct);
            Theme.WithIcon(btnDeleteProduct, "🗑️", primary: false);
            btnDeleteProduct.Click += BtnDeleteProduct_Click;
            Controls.Add(btnDeleteProduct);

            // DataGridView productos
            dgvProducts = new DataGridView
            {
                Left = 10,
                Top = 120,
                Width = Math.Max(600, this.ClientSize.Width - 40),
                Height = Math.Max(200, this.ClientSize.Height - 140),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false
            };

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", DataPropertyName = "Id", HeaderText = "Id", Width = 60 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Barcode", DataPropertyName = "Barcode", HeaderText = "Barcode", Width = 160 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", DataPropertyName = "Nombre", HeaderText = "Nombre", Width = 300 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", DataPropertyName = "Precio", HeaderText = "Precio", Width = 100 });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stock", DataPropertyName = "Stock", HeaderText = "Stock", Width = 80 });

            Controls.Add(dgvProducts);

            // Output textbox (debajo)
            txtOutput = new TextBox { Left = 10, Top = 130 + dgvProducts.Height, Width = 920, Height = 120, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom };
            txtOutput.Font = new Font("Consolas", 10);
            Controls.Add(txtOutput);

            this.Resize += (s, e) =>
            {
                dgvProducts.Width = Math.Max(400, this.ClientSize.Width - 20);
                dgvProducts.Height = Math.Max(200, this.ClientSize.Height - 260);
                txtOutput.Top = 130 + dgvProducts.Height;
                txtOutput.Width = Math.Max(300, this.ClientSize.Width - 20);
                txtOutput.Height = Math.Max(80, this.ClientSize.Height - txtOutput.Top - 20);
            };
        }

        // Productos: carga
        public void LoadProducts()
        {
            try
            {
                var list = db.Products.OrderBy(p => p.Nombre).ToList();
                dgvProducts.DataSource = null;
                dgvProducts.DataSource = list;
            }
            catch (Exception ex)
            {
                txtOutput.Text = "Error cargando productos: " + ex.Message;
            }
        }

        private void BtnAddProduct_Click(object? sender, EventArgs e)
        {
            var dlg = new ProductDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // evitar barcode duplicado
                var existing = db.Products.SingleOrDefault(p => p.Barcode == dlg.Product.Barcode);
                if (existing != null)
                {
                    MessageBox.Show("Ya existe un producto con ese barcode.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                db.Products.Add(dlg.Product);
                db.SaveChanges();
                LoadProducts();
            }
        }

        private void BtnEditProduct_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) { MessageBox.Show("Selecciona un producto."); return; }
            var prod = (Product)dgvProducts.CurrentRow.DataBoundItem!;
            var prodFromDb = db.Products.Find(prod.Id);
            if (prodFromDb == null) return;
            var dlg = new ProductDialog(prodFromDb);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                db.SaveChanges();
                LoadProducts();
            }
        }

        private void BtnDeleteProduct_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) { MessageBox.Show("Selecciona un producto."); return; }
            var prod = (Product)dgvProducts.CurrentRow.DataBoundItem!;
            if (MessageBox.Show($"Eliminar producto {prod.Nombre}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var p = db.Products.Find(prod.Id);
                if (p != null)
                {
                    db.Products.Remove(p);
                    db.SaveChanges();
                    LoadProducts();
                }
            }
        }

        // Mantengo tus funciones de reportes/backups abajo (las dejé intactas)
        public void GenerateCsvTotals()
        {
            try
            {
                var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MiPOS");
                var backupsDir = Path.Combine(baseDir, "backups");
                Directory.CreateDirectory(backupsDir);

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
                txtOutput.Text = $"CSV generado: {csvPath}";
            }
            catch (Exception ex)
            {
                txtOutput.Text = "Error al generar CSV: " + ex.Message;
            }
        }

        public void BackupDb()
        {
            try
            {
                var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MiPOS");
                var backupsDir = Path.Combine(baseDir, "backups");
                Directory.CreateDirectory(backupsDir);

                var dataFolder = Path.Combine(baseDir, "data");
                var dbPath = Path.Combine(dataFolder, "pos.db");
                if (File.Exists(dbPath))
                {
                    var dbBackup = Path.Combine(backupsDir, $"pos_db_backup_{DateTime.Now:yyyyMM_ddHHmmss}.db");
                    File.Copy(dbPath, dbBackup, true);
                    txtOutput.Text = $"Backup copiado a: {dbBackup}";
                }
                else txtOutput.Text = "No se encontró el archivo pos.db";
            }
            catch (Exception ex)
            {
                txtOutput.Text = "Error al generar backup: " + ex.Message;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db?.Dispose();
            base.Dispose(disposing);
        }
    }
}
