using GestorEventosMusicales.Data;
using GestorEventosMusicales.Modelos;

namespace GestorEventosMusicales.Paginas
{
    public partial class AddLocationPage : ContentPage, IQueryAttributable
    {
        private Locacion locacionEditando = null;
        private readonly DatabaseService _databaseService;

        public AddLocationPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = this;

            // Limpiar cualquier valor anterior si es necesario
            if (locacionEditando == null)
            {
                LimpiarCampos();
            }
        }

        // Manejo de parámetros recibidos por navegación
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            // Verifica si el parámetro "Locacion" está presente para editar
            if (query.ContainsKey("Locacion"))
            {
                locacionEditando = query["Locacion"] as Locacion;

                if (locacionEditando != null)
                {
                    CargarCampos(locacionEditando); // Si es una edición, carga los datos
                }
                else
                {
                    LimpiarCampos(); // Si el parámetro no es válido, limpiamos
                }
            }
            else
            {
                locacionEditando = null;
                LimpiarCampos();
            }
        }

        // Método para cargar los campos cuando editamos una locación
        private void CargarCampos(Locacion locacion)
        {
            nombreEntry.Text = locacion.Nombre;
            direccionEntry.Text = locacion.Direccion;
            ciudadEntry.Text = locacion.Ciudad;
            regionEntry.Text = locacion.Region;
            codigoPostalEntry.Text = locacion.CodigoPostal;
            paisEntry.Text = locacion.Pais;
            capacidadEntry.Text = locacion.Capacidad.ToString();
            telefonoEntry.Text = locacion.Telefono;
            emailEntry.Text = locacion.Email;
        }

        // Método para guardar los cambios de una locación (creación o actualización)
        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            try
            {
                var nuevaLocacion = new Locacion
                {
                    Id = locacionEditando?.Id ?? 0,
                    Nombre = nombreEntry.Text,
                    Direccion = direccionEntry.Text,
                    Ciudad = ciudadEntry.Text,
                    Region = regionEntry.Text,
                    CodigoPostal = codigoPostalEntry.Text,
                    Pais = paisEntry.Text,
                    Capacidad = int.TryParse(capacidadEntry.Text, out int capacidad) ? capacidad : 0,
                    Telefono = telefonoEntry.Text,
                    Email = emailEntry.Text
                };

                int resultado;

                if (locacionEditando != null)
                {
                    // Si estamos editando, actualizamos la locación
                    int managerId = await _databaseService.ObtenerManagerIdActualAsync();
                    resultado = await _databaseService.ActualizarLocacionAsync(nuevaLocacion, managerId);
                }
                else
                {
                    // Si estamos creando una nueva locación
                    int managerId = await _databaseService.ObtenerManagerIdActualAsync();
                    resultado = await _databaseService.InsertarLocacionAsync(nuevaLocacion, managerId);
                }

                if (resultado > 0)
                {
                    await DisplayAlert("Éxito", locacionEditando != null ? "Lugar actualizado correctamente." : "Lugar agregado correctamente.", "OK");
                    await Shell.Current.GoToAsync($"///{nameof(ViewEditLocationPage)}");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo guardar el lugar.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }

        // Método para limpiar los campos de la página
        private void LimpiarCampos()
        {
            nombreEntry.Text = string.Empty;
            direccionEntry.Text = string.Empty;
            ciudadEntry.Text = string.Empty;
            regionEntry.Text = string.Empty;
            codigoPostalEntry.Text = string.Empty;
            paisEntry.Text = string.Empty;
            capacidadEntry.Text = string.Empty;
            telefonoEntry.Text = string.Empty;
            emailEntry.Text = string.Empty;
        }

        // Método para cancelar y volver a la página anterior
        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            // Navegar hacia la página de vista y edición
            await Shell.Current.GoToAsync($"///{nameof(ViewEditLocationPage)}");
        }
    }
}
