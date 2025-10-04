using System;

namespace MiPOS.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public decimal Monto { get; set; }
    }
}
