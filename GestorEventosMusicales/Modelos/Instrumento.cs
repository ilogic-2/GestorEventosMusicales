using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestorEventosMusicales.Modelos
{
    public class Instrumento
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public string Proveedor { get; set; }
        public int ManagerId { get; set; }
    }
}
