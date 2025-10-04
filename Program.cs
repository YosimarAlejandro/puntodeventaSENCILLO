// namespace MiPOS;

// static class Program
// {
//     /// <summary>
//     ///  The main entry point for the application.
//     /// </summary>
//     [STAThread]
//     static void Main()
//     {
//         // To customize application configuration such as set high DPI settings or default font,
//         // see https://aka.ms/applicationconfiguration.
//         ApplicationConfiguration.Initialize();
//         Application.Run(new Form1());
//     }    
// }
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
            Application.Run(new MainForm());
        }
    }
}
