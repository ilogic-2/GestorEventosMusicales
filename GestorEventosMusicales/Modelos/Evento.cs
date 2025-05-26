using System;

namespace GestorEventosMusicales.Modelos
{
    public class Evento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaEvento { get; set; }
        public DateTime FechaMontaje { get; set; }

        public int LocacionId { get; set; }
        public Locacion? Locacion { get; set; }

        public List<Manager> Managers { get; set; } = new List<Manager>();
        public List<Instrumento> Instrumentos { get; set; } = new List<Instrumento>();
        public List<Artista> Artistas { get; set; } = new List<Artista>();
    }
}
