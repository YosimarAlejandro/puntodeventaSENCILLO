using System;
using System.Windows.Forms;
using MiPOS.Views.Sales;
using MiPOS.Views.Admin;
using System.Drawing;

namespace MiPOS
{
    public class MainForm : Form
    {
        // UI
        private MenuStrip menu = null!;
        private ToolStripMenuItem menuArchivo = null!;
        private ToolStripMenuItem menuVentas = null!;
        private ToolStripMenuItem menuAdmin = null!;
        private Panel sidebar = null!;
        private Panel mainArea = null!;
        private Button btnSideNewSale = null!;
        private Button btnSideBackup = null!;
        private Label lblUserInfo = null!;

        // UserControls
        private SalesViewControl salesView = null!;
        private AdminViewControl adminView = null!;

        // Estado
        private readonly string role;
        private readonly string username;
        public bool LogoutRequested { get; private set; } = false;

        public MainForm(string role, string username)
        {
            this.role = role;
            this.username = username;

            InitializeComponent();

            // Crear las vistas
            salesView = new SalesViewControl(role, username);
            adminView = new AdminViewControl();

            ShowSalesView(); // vista por defecto
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
            var mnuNueva = new ToolStripMenuItem("Nueva venta", null, (s, e) => ShowSalesView());
            var mnuListado = new ToolStripMenuItem("Listado ventas", null, (s, e) => ShowSalesView());
            menuVentas.DropDownItems.Add(mnuNueva);
            menuVentas.DropDownItems.Add(mnuListado);

            menuAdmin = new ToolStripMenuItem("Administración");
            var mnuReportes = new ToolStripMenuItem("Reportes (CSV)", null, (s, e) => { adminView.GenerateCsvTotals(); });
            var mnuRespaldos = new ToolStripMenuItem("Respaldos (BD)", null, (s, e) => { adminView.BackupDb(); });
            menuAdmin.DropDownItems.Add(mnuReportes);
            menuAdmin.DropDownItems.Add(mnuRespaldos);

            menu.Items.Add(menuArchivo);
            menu.Items.Add(menuVentas);
            menu.Items.Add(menuAdmin);
            Controls.Add(menu);

            // Sidebar
            sidebar = new Panel { Left = 0, Top = menu.Height, Width = 180, Height = ClientSize.Height - menu.Height, BackColor = Color.FromArgb(45, 45, 48), Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left };
            btnSideNewSale = new Button { Text = "Nueva venta", Left = 10, Top = 20, Width = 160, Height = 40, FlatStyle = FlatStyle.Flat };
            btnSideNewSale.Click += (s, e) => ShowSalesView();
            btnSideBackup = new Button { Text = "Respaldos", Left = 10, Top = 70, Width = 160, Height = 40, FlatStyle = FlatStyle.Flat };
            btnSideBackup.Click += (s, e) => { if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase)) { ShowAdminView(); } else MessageBox.Show("Acceso denegado"); };

            lblUserInfo = new Label { Left = 10, Top = 520, Width = 160, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
            lblUserInfo.Text = $"{username}\n({role})";

            foreach (Control c in new Control[] { btnSideNewSale, btnSideBackup, lblUserInfo })
            {
                c.BackColor = Color.FromArgb(63, 63, 70);
                c.ForeColor = Color.White;
            }

            sidebar.Controls.Add(btnSideNewSale);
            sidebar.Controls.Add(btnSideBackup);
            sidebar.Controls.Add(lblUserInfo);
            Controls.Add(sidebar);

            // mainArea
            mainArea = new Panel { Left = sidebar.Right + 10, Top = menu.Height + 10, Width = ClientSize.Width - sidebar.Width - 30, Height = ClientSize.Height - menu.Height - 30, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, BorderStyle = BorderStyle.None };
            Controls.Add(mainArea);

            this.Resize += (s, e) =>
            {
                sidebar.Height = ClientSize.Height - menu.Height;
                mainArea.Width = ClientSize.Width - sidebar.Width - 30;
                mainArea.Height = ClientSize.Height - menu.Height - 30;
            };
        }

        private void ApplyRoleVisibility()
        {
            menuAdmin.Visible = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            btnSideBackup.Visible = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        private void ShowSalesView()
        {
            mainArea.Controls.Clear();
            salesView.Dock = DockStyle.Fill;
            mainArea.Controls.Add(salesView);
            salesView.LoadSales();
        }

        private void ShowAdminView()
        {
            mainArea.Controls.Clear();
            adminView.Dock = DockStyle.Fill;
            mainArea.Controls.Add(adminView);
        }

        private void MnuCerrarSesion_Click(object? sender, EventArgs e)
        {
            this.LogoutRequested = true;
            this.Close();
        }

        private void MnuSalir_Click(object? sender, EventArgs e)
        {
            this.LogoutRequested = false;
            Application.Exit();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
        }
    }
}
