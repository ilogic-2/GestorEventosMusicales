namespace GestorEventosMusicales.Modelos
{
    public class Manager
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Contrasena { get; set; }

        public byte[] Imagen { get; set; }

        public List<Artista> Artistas { get; set; }

        public Manager() { }

        public Manager(int id, string nombre, string correo, string telefono, string contrasena = "", byte[] imagen = null)
        {
            Id = id;
            Nombre = nombre;
            Correo = correo;
            Telefono = telefono;
            Contrasena = contrasena;
            Imagen = imagen;
        }
    }
}
