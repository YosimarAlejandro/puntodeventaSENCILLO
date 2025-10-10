using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MiPOS.Models;

namespace MiPOS.Views.Admin
{
    public class ProductDialog : Form
    {
        TextBox txtBarcode = null!;
        TextBox txtNombre = null!;
        TextBox txtPrecio = null!;
        TextBox txtStock = null!;
        Button btnOk = null!;
        Button btnCancel = null!;

        public Product Product { get; private set; } = null!;

        public ProductDialog(Product? product = null)
        {
            Product = product ?? new Product();
            InitializeComponent();

            if (product != null)
            {
                txtBarcode.Text = product.Barcode;
                txtNombre.Text = product.Nombre;
                txtPrecio.Text = product.Precio.ToString("F2", CultureInfo.InvariantCulture);
                txtStock.Text = product.Stock.ToString();
            }
        }

        private void InitializeComponent()
        {
            Text = "Producto";
            Width = 520;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            // Labels + Inputs (alineados)
            var leftLabel = 20;
            var leftInput = 160;
            var labelWidth = 120;
            var inputWidth = 320;
            var top = 20;
            var vSpacing = 40;

            var lblBarcode = new Label { Left = leftLabel, Top = top, Text = "Código de barras", Width = labelWidth };
            txtBarcode = new TextBox { Left = leftInput, Top = top - 2, Width = inputWidth, MaxLength = 64 };

            top += vSpacing;
            var lblNombre = new Label { Left = leftLabel, Top = top, Text = "Nombre", Width = labelWidth };
            txtNombre = new TextBox { Left = leftInput, Top = top - 2, Width = inputWidth };

            top += vSpacing;
            var lblPrecio = new Label { Left = leftLabel, Top = top, Text = "Precio", Width = labelWidth };
            txtPrecio = new TextBox { Left = leftInput, Top = top - 2, Width = 160 };
            txtPrecio.KeyPress += TxtNumeric_KeyPress;

            top += vSpacing;
            var lblStock = new Label { Left = leftLabel, Top = top, Text = "Stock", Width = labelWidth };
            txtStock = new TextBox { Left = leftInput, Top = top - 2, Width = 100 };
            txtStock.KeyPress += TxtInteger_KeyPress;

            // Botones
            btnOk = new Button { Text = "Guardar", Left = leftInput, Top = top + vSpacing + 10, Width = 120, Height = 34 };
            btnOk.Click += BtnOk_Click;

            btnCancel = new Button { Text = "Cancelar", Left = leftInput + 140, Top = top + vSpacing + 10, Width = 120, Height = 34 };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Habilita Enter/Esc
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            Controls.AddRange(new Control[] {
                lblBarcode, txtBarcode,
                lblNombre, txtNombre,
                lblPrecio, txtPrecio,
                lblStock, txtStock,
                btnOk, btnCancel
            });
        }

        // Permite sólo dígitos y separador decimal (.),(,) para precio
        private void TxtNumeric_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.' && e.KeyChar != ',')
                e.Handled = true;

            var tb = sender as TextBox;
            if (tb != null && (e.KeyChar == '.' || e.KeyChar == ','))
            {
                // sólo permitir una coma o punto
                if (tb.Text.Contains('.') || tb.Text.Contains(','))
                    e.Handled = true;
            }
        }

        // Permite sólo enteros (stock)
        private void TxtInteger_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            // Normalizamos el precio: aceptar "12.34" o "12,34"
            var precioText = txtPrecio.Text.Trim().Replace(',', '.');

            if (string.IsNullOrWhiteSpace(txtBarcode.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text) ||
                !decimal.TryParse(precioText, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal precio))
            {
                MessageBox.Show("Por favor, complete todos los campos correctamente (barcode, nombre, precio).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Product.Barcode = txtBarcode.Text.Trim();
            Product.Nombre = txtNombre.Text.Trim();
            Product.Precio = precio;
            Product.Stock = int.TryParse(txtStock.Text.Trim(), out int s) ? s : 0;

            this.DialogResult = DialogResult.OK;
        }
    }
}
