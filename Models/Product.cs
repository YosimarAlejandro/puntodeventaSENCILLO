using System;
namespace MiPOS.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = null!;
        public string Nombre{ get; set; } = null!;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ClaveSAP { get; set; }
    }
}