using System;
using System.Windows.Forms;

namespace MiPOS
{
    public class LoginForm : Form
    {
        TextBox txtUser = null!;
        TextBox txtPass = null!;
        Button btnLogin = null!;
        Button btnCancelar = null!;

        public bool Authenticated { get; private set; } = false;
        public string? Role { get; private set; }
        public string? Username { get; private set; }

        public LoginForm()
        {
            Text = "MiPOS - Login";
            Width = 360;
            Height = 200;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            var lblUser = new Label { Left = 20, Top = 20, Text = "Usuario", Width = 80 };
            txtUser = new TextBox { Left = 110, Top = 18, Width = 200 };
            var lblPass = new Label { Left = 20, Top = 60, Text = "Contraseña", Width = 80 };
            txtPass = new TextBox { Left = 110, Top = 58, Width = 200, UseSystemPasswordChar = true };

            btnLogin = new Button { Text = "Entrar", Left = 110, Width = 90, Top = 100 };
            btnLogin.Click += BtnLogin_Click;

            btnCancelar = new Button { Text = "Cancelar", Left = 220, Width = 90, Top = 100 };
            btnCancelar.Click += (s, e) => { Authenticated = false; this.Close(); };

            Controls.Add(lblUser);
            Controls.Add(txtUser);
            Controls.Add(lblPass);
            Controls.Add(txtPass);
            Controls.Add(btnLogin);
            Controls.Add(btnCancelar);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var user = txtUser.Text.Trim();
            var pass = txtPass.Text.Trim();

            // Credenciales hardcodeadas (según tu petición)
            if (user.Equals("admin", StringComparison.OrdinalIgnoreCase) && pass == "1234")
            {
                Authenticated = true;
                Role = "Admin";
                Username = "admin";
                this.Close();
                return;
            }

            if (user.Equals("vendedor", StringComparison.OrdinalIgnoreCase) && pass == "1234")
            {
                Authenticated = true;
                Role = "Vendedor";
                Username = "vendedor";
                this.Close();
                return;
            }

            MessageBox.Show("Usuario o contraseña incorrectos.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPass.Clear();
            txtPass.Focus();
        }
    }
}
