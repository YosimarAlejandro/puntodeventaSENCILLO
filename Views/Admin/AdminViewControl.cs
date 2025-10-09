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

        public AdminViewControl()
        {
            InitializeComponent();
            db = new PosDbContext();
        }

        private void InitializeComponent()
        {
            Dock = DockStyle.Fill;
            BackColor = Theme.PanelLight;

            btnGenCsv = new Button { Left = 10, Top = 10, Width = 220, Height = 44, Text = "Generar reportes (CSV)" };
            Theme.StylePrimaryButton(btnGenCsv);
            Theme.WithIcon(btnGenCsv, "📊");
            btnGenCsv.Click += (s, e) => GenerateCsvTotals();
            Controls.Add(btnGenCsv);

            btnBackupDb = new Button { Left = 250, Top = 10, Width = 220, Height = 44, Text = "Crear respaldo BD" };
            Theme.StyleSecondaryButton(btnBackupDb);
            Theme.WithIcon(btnBackupDb, "💾", primary: false);
            btnBackupDb.Click += (s, e) => BackupDb();
            Controls.Add(btnBackupDb);

            txtOutput = new TextBox { Left = 10, Top = 70, Width = 920, Height = 420, Multiline = true, ScrollBars = ScrollBars.Vertical, ReadOnly = true, Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right };
            txtOutput.Font = new Font("Consolas", 10);
            Controls.Add(txtOutput);

            this.Resize += (s, e) =>
            {
                txtOutput.Width = Math.Max(300, this.ClientSize.Width - 20);
                txtOutput.Height = Math.Max(200, this.ClientSize.Height - 80);
            };
        }

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
