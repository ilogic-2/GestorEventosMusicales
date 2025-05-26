using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestorEventosMusicales.Modelos
{
    public class Artista
    {
        public int Id { get; set; } 
        public string Nombre { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string Nacionalidad { get; set; }
        public string Banda { get; set; }

        public byte[] Imagen { get; set; }  

        // calcular edad
        public int Edad => DateTime.Now.Year - FechaNacimiento.Year -
            (DateTime.Now.DayOfYear < FechaNacimiento.DayOfYear ? 1 : 0);

        public ImageSource ImagenPreview { get; internal set; }

        public List<Manager> Managers { get; set; }
    }
}