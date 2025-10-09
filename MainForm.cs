using System;
using System.Linq;
using System.Windows.Forms;
using MiPOS.Models;
using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Drawing;

namespace MiPOS
{
    public class MainForm : Form
    {
        // UI
        private MenuStrip menu;
        private ToolStripMenuItem menuArchivo, menuVentas, menuAdmin;
        private Panel sidebar;
        private Panel mainArea;
        private Button btnSideNewSale, btnSideBackup;
        private Label lblUserInfo;

        // Sales UI (contenido principal por defecto)
        private TextBox txtMonto;
        private Button btnAgregar, btnActualizar, btnBorrar;
        private ListView lvSales;
        private PosDbContext db;

        // Estado
        private readonly string role;
        private readonly string username;
        public bool LogoutRequested { get; private set; } = false;

        public MainForm(string role, string username)
        {
            this.role = role;
            this.username = username;

            InitializeComponent();
            db = new PosDbContext();
            LoadSales();
            ApplyRoleVisibility();
        }

        private void InitializeComponent()
        {
            Text = $"MiPOS - Usuario: {username} | Rol: {role}";
            Width = 1000;
            Height = 640;
            StartPosition = FormStartPosition.CenterScreen;

            // MenuStrip superior
            menu = new MenuStrip();
            menuArchivo = new ToolStripMenuItem("Archivo");
            var mnuCerrarSesion = new ToolStripMenuItem("Cerrar sesión", null, MnuCerrarSesion_Click);
            var mnuSalir = new ToolStripMenuItem("Salir", null, MnuSalir_Click);
            menuArchivo.DropDownItems.Add(mnuCerrarSesion);
            menuArchivo.DropDownItems.Add(new ToolStripSeparator());
            menuArchivo.DropDownItems.Add(mnuSalir);

            menuVentas = new ToolStripMenuItem("Ventas");
            var mnuNueva = new ToolStripMenuItem("Nueva venta", null, (s,e)=> ShowSalesView());
            var mnuListado = new ToolStripMenuItem("Listado ventas", null, (s,e)=> ShowSalesView());
            menuVentas.DropDownItems.Add(mnuNueva);
            menuVentas.DropDownItems.Add(mnuListado);

            menuAdmin = new ToolStripMenuItem("Administración");
            var mnuReportes = new ToolStripMenuItem("Reportes (CSV)", null, (s,e)=> GenerateCsvTotals());
            var mnuRespaldos = new ToolStripMenuItem("Respaldos (BD)", null, (s,e)=> BackupDb());
            menuAdmin.DropDownItems.Add(mnuReportes);
            menuAdmin.DropDownItems.Add(mnuRespaldos);

            menu.Items.Add(menuArchivo);
            menu.Items.Add(menuVentas);
            menu.Items.Add(menuAdmin);
            Controls.Add(menu);

            // Sidebar
            sidebar = new Panel { Left = 0, Top = menu.Height, Width = 180, Height = ClientSize.Height - menu.Height, BackColor = Color.FromArgb(45,45,48), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left };
            btnSideNewSale = new Button { Text = "Nueva venta", Left = 10, Top = 20, Width = 160, Height = 40, FlatStyle = FlatStyle.Flat };
            btnSideNewSale.Click += (s,e)=> ShowSalesView();
            btnSideBackup = new Button { Text = "Respaldos", Left = 10, Top = 70, Width = 160, Height = 40, FlatStyle = FlatStyle.Flat };
            btnSideBackup.Click += (s,e)=> { if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) BackupAll(); else MessageBox.Show("Acceso denegado"); };

            lblUserInfo = new Label { Left = 10, Top = 520, Width = 160, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            lblUserInfo.Text = $"{username}\n({role})";

            // Ajustes visuales sidebar
            foreach (Control c in new Control[] { btnSideNewSale, btnSideBackup, lblUserInfo })
            {
                c.BackColor = Color.FromArgb(63,63,70);
                c.ForeColor = Color.White;
            }

            sidebar.Controls.Add(btnSideNewSale);
            sidebar.Controls.Add(btnSideBackup);
            sidebar.Controls.Add(lblUserInfo);
            Controls.Add(sidebar);

            // Área principal
            mainArea = new Panel { Left = sidebar.Right + 10, Top = menu.Height + 10, Width = ClientSize.Width - sidebar.Width - 30, Height = ClientSize.Height - menu.Height - 30, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.None };
            Controls.Add(mainArea);

            // Construir Sales view dentro de mainArea
            BuildSalesView();

            // Manejo resize para que sidebar y mainArea se ajusten
            this.Resize += (s,e) =>
            {
                sidebar.Height = ClientSize.Height - menu.Height;
                mainArea.Width = ClientSize.Width - sidebar.Width - 30;
                mainArea.Height = ClientSize.Height - menu.Height - 30;
            };
        }

        private void ApplyRoleVisibility()
        {
            // Solo Admin ve el menuAdmin
            menuAdmin.Visible = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            btnSideBackup.Visible = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        #region Sales view build & handlers
        private void BuildSalesView()
        {
            mainArea.Controls.Clear();

            // Monto input grande
            txtMonto = new TextBox { Left = 10, Top = 10, Width = 360, Font = new Font("Segoe UI", 20), Anchor = AnchorStyles.Top | AnchorStyles.Left };
            txtMonto.KeyPress += TxtMonto_KeyPress;
            mainArea.Controls.Add(txtMonto);

            btnAgregar = new Button { Left = 380, Top = 10, Width = 120, Height = 45, Text = "Agregar" , Anchor = AnchorStyles.Top | AnchorStyles.Left};
            btnAgregar.Click += BtnAgregar_Click;
            mainArea.Controls.Add(btnAgregar);

            btnActualizar = new Button { Left = 510, Top = 10, Width = 120, Height = 45, Text = "Actualizar", Anchor = AnchorStyles.Top | AnchorStyles.Left };
            btnActualizar.Click += BtnActualizar_Click;
            mainArea.Controls.Add(btnActualizar);

            btnBorrar = new Button { Left = 640, Top = 10, Width = 120, Height = 45, Text = "Borrar", Anchor = AnchorStyles.Top | AnchorStyles.Left };
            btnBorrar.Click += BtnBorrar_Click;
            mainArea.Controls.Add(btnBorrar);

            // ListView ventas
            lvSales = new ListView { Left = 10, Top = 70, Width = mainArea.Width - 20, Height = mainArea.Height - 90, View = View.Details, FullRowSelect = true, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            lvSales.Columns.Add("Id", 60);
            lvSales.Columns.Add("Fecha", 200);
            lvSales.Columns.Add("Monto", 120);
            lvSales.SelectedIndexChanged += LvSales_SelectedIndexChanged;
            mainArea.Controls.Add(lvSales);
        }

        private void LoadSales()
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
            var txt = txtMonto.Text.Trim().Replace(',', '.');
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
        #endregion

        #region Menu / Sidebar actions
        private void ShowSalesView()
        {
            BuildSalesView();
            LoadSales();
        }

        private void ShowAdminView()
        {
            mainArea.Controls.Clear();

            var lbl = new Label { Left = 10, Top = 10, Width = mainArea.Width - 20, Text = "Panel de Administración", Font = new Font("Segoe UI", 16, FontStyle.Bold) , Anchor = AnchorStyles.Top | AnchorStyles.Left};
            mainArea.Controls.Add(lbl);

            var btnGenCsv = new Button { Left = 10, Top = 60, Width = 220, Height = 40, Text = "Generar reportes (CSV)"};
            btnGenCsv.Click += (s,e) => GenerateCsvTotals();
            mainArea.Controls.Add(btnGenCsv);

            var btnBackup = new Button { Left = 240, Top = 60, Width = 220, Height = 40, Text = "Crear respaldo BD" };
            btnBackup.Click += (s,e) => BackupDb();
            mainArea.Controls.Add(btnBackup);

            var lblInfo = new Label { Left = 10, Top = 120, Width = mainArea.Width - 20, Height = 200, Text = "Aquí puedes agregar futuras herramientas: gestión de usuarios, ajustes, reportes avanzados, etc.", Anchor = AnchorStyles.Top | AnchorStyles.Left};
            mainArea.Controls.Add(lblInfo);
        }

        private void MnuCerrarSesion_Click(object? sender, EventArgs e)
        {
            // Indica que el usuario quiere volver a login
            this.LogoutRequested = true;
            this.Close();
        }

        private void MnuSalir_Click(object? sender, EventArgs e)
        {
            this.LogoutRequested = false;
            Application.Exit();
        }
        #endregion

        #region Backups / CSV
        private void GenerateCsvTotals()
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
                MessageBox.Show($"CSV generado:\n{csvPath}", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar CSV: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BackupDb()
        {
            try
            {
                var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MiPOS");
                var backupsDir = Path.Combine(baseDir, "backups");
                Directory.CreateDirectory(backupsDir);

                var dataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "MiPOS", "data");
                var dbPath = Path.Combine(dataFolder, "pos.db");
                if (File.Exists(dbPath))
                {
                    var dbBackup = Path.Combine(backupsDir, $"pos_db_backup_{DateTime.Now:yyyyMM_ddHHmmss}.db");
                    File.Copy(dbPath, dbBackup, true);
                    MessageBox.Show($"Backup copiado a:\n{dbBackup}", "Backup OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else MessageBox.Show("No se encontró el archivo pos.db", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al generar backup: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Atajo desde sidebar (usa BackupDb internamente)
        private void BackupAll()
        {
            // Genera CSV y copia de BD
            GenerateCsvTotals();
            BackupDb();
        }
        #endregion

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            db?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
