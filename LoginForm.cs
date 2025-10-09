using System;
using System.Windows.Forms;
using MiPOS.UI;
using System.Drawing;

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
            Width = 380;
            Height = 240;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MiPOS - Login";

            InitializeComponent();
            Theme.Apply(this);
        }

        private void InitializeComponent()
        {
            var lblUser = new Label { Left = 20, Top = 20, Text = "Usuario", Width = 80 };
            lblUser.Font = Theme.UiFontBold;
            Controls.Add(lblUser);

            txtUser = new TextBox { Left = 110, Top = 18, Width = 230 };
            Controls.Add(txtUser);

            var lblPass = new Label { Left = 20, Top = 60, Text = "Contraseña", Width = 80 };
            lblPass.Font = Theme.UiFontBold;
            Controls.Add(lblPass);

            txtPass = new TextBox { Left = 110, Top = 58, Width = 230, UseSystemPasswordChar = true };
            Controls.Add(txtPass);

            btnLogin = new Button { Text = "Entrar", Left = 110, Width = 110, Top = 110, Height = 50 };
            Theme.StylePrimaryButton(btnLogin);
            Theme.WithIcon(btnLogin, "🔐");
            btnLogin.Click += BtnLogin_Click;
            Controls.Add(btnLogin);

            btnCancelar = new Button { Text = "Cancelar", Left = 230, Width = 120, Top = 110, Height = 50 };
            Theme.StyleSecondaryButton(btnCancelar);
            btnCancelar.Click += (s, e) => { Authenticated = false; this.Close(); };
            Controls.Add(btnCancelar);
        }

        private void BtnLogin_Click(object? sender, EventArgs e)
        {
            var user = txtUser.Text.Trim();
            var pass = txtPass.Text.Trim();

            // Credenciales hardcodeadas
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
