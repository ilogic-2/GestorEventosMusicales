using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestorEventosMusicales.Modelos
{
    public class Locacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Region { get; set; }
        public string CodigoPostal { get; set; }
        public string Pais { get; set; }
        public int Capacidad { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public int ManagerId { get; set; } 
        public bool IsVisible { get; internal set; }
    }

}
