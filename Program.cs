using System;
using System.Windows.Forms;
using MiPOS.Models;
using Microsoft.EntityFrameworkCore;

namespace MiPOS
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Crear DB si no existe y activar WAL
            using (var db = new PosDbContext())
            {
                db.Database.EnsureCreated();
                db.Database.ExecuteSqlRaw("PRAGMA journal_mode = WAL;");
            }

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            while (true) // permite logout y volver a login
            {
                using (var login = new LoginForm())
                {
                    var res = login.ShowDialog();
                    if (!login.Authenticated)
                    {
                        // Si cerró o canceló, salir de la app
                        break;
                    }

                    // Si autenticado, abrir MainForm
                    var main = new MainForm(login.Role!, login.Username!);
                    Application.Run(main);

                    // Si MainForm solicitó logout, repetir ciclo (volver a login)
                    if (main.LogoutRequested)
                        continue;

                    // Si MainForm cerró sin pedir logout (p. ej. Salir), terminar app
                    break;
                }
            }
        }
    }
}
