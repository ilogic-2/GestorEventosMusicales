using System.Collections.ObjectModel;
using GestorEventosMusicales.Data;
using GestorEventosMusicales.Modelos;

namespace GestorEventosMusicales.Paginas
{
    public partial class CreateEventPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<Artista> ArtistasSeleccionados { get; set; }
        public ObservableCollection<Instrumento> InstrumentosSeleccionados { get; set; }
        public ObservableCollection<Manager> OrganizadoresSeleccionados { get; set; }

        private List<Artista> todosLosArtistas;
        private List<Instrumento> todosLosInstrumentos;
        private List<Locacion> locacionesDisponibles;
        private List<Manager> managersDisponibles;

        public CreateEventPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            ArtistasSeleccionados = new ObservableCollection<Artista>();
            InstrumentosSeleccionados = new ObservableCollection<Instrumento>();
            OrganizadoresSeleccionados = new ObservableCollection<Manager>();
            artistasCollection.ItemsSource = ArtistasSeleccionados;
            instrumentosCollection.ItemsSource = InstrumentosSeleccionados;
            organizadoresCollection.ItemsSource = OrganizadoresSeleccionados;
            CargarDatos();
        }

        private async void CargarDatos()
        {
            try
            {
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();

                // Obtener las locaciones de la base de datos y asignarlas al picker
                locacionesDisponibles = await _databaseService.ObtenerLocacionesAsync(managerId);

                // Asignación a la lista de locaciones al Picker, con la propiedad "Nombre" para mostrar
                locacionPicker.ItemsSource = locacionesDisponibles;
                locacionPicker.ItemDisplayBinding = new Binding("Nombre"); 

                managersDisponibles = await _databaseService.ObtenerManagersAsync();

                todosLosArtistas = await _databaseService.ObtenerArtistasAsync(managerId);
                todosLosInstrumentos = await _databaseService.ObtenerInstrumentosAsync(managerId);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar datos: {ex.Message}", "OK");
            }
        }

        private async void OnAgregarArtistaClicked(object sender, EventArgs e)
        {
            var disponibles = todosLosArtistas.Where(a => !ArtistasSeleccionados.Any(s => s.Id == a.Id)).ToList();
            if (disponibles.Count == 0)
            {
                await DisplayAlert("Info", "No hay más artistas disponibles para agregar.", "OK");
                return;
            }

            var seleccion = await DisplayActionSheet("Selecciona un artista", "Cancelar", null, disponibles.Select(a => a.Nombre).ToArray());

            if (seleccion != null && seleccion != "Cancelar")
            {
                var artista = disponibles.FirstOrDefault(a => a.Nombre == seleccion);
                if (artista != null)
                    ArtistasSeleccionados.Add(artista);
            }
        }

        private async void OnAgregarInstrumentoClicked(object sender, EventArgs e)
        {
            var disponibles = todosLosInstrumentos.Where(i => !InstrumentosSeleccionados.Any(s => s.Id == i.Id)).ToList();
            if (disponibles.Count == 0)
            {
                await DisplayAlert("Info", "No hay más instrumentos disponibles para agregar.", "OK");
                return;
            }

            var seleccion = await DisplayActionSheet("Selecciona un instrumento", "Cancelar", null, disponibles.Select(i => i.Nombre).ToArray());

            if (seleccion != null && seleccion != "Cancelar")
            {
                var instrumento = disponibles.FirstOrDefault(i => i.Nombre == seleccion);
                if (instrumento != null)
                    InstrumentosSeleccionados.Add(instrumento);
            }
        }



        private async void OnAgregarOrganizadorClicked(object sender, EventArgs e)
        {
            var disponibles = managersDisponibles.Where(m => !OrganizadoresSeleccionados.Any(s => s.Id == m.Id)).ToList();
            if (disponibles.Count == 0)
            {
                await DisplayAlert("Info", "No hay más organizadores disponibles para agregar.", "OK");
                return;
            }

            var seleccion = await DisplayActionSheet("Selecciona un organizador", "Cancelar", null, disponibles.Select(m => m.Nombre).ToArray());

            if (seleccion != null && seleccion != "Cancelar")
            {
                var manager = disponibles.FirstOrDefault(m => m.Nombre == seleccion);
                if (manager != null)
                    OrganizadoresSeleccionados.Add(manager);
            }
        }

        private void OnQuitarArtistaClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var artista = button?.BindingContext as Artista;

            if (artista != null && ArtistasSeleccionados.Contains(artista))
            {
                ArtistasSeleccionados.Remove(artista);
            }
        }

        private void OnQuitarInstrumentoClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var instrumento = button?.BindingContext as Instrumento;

            if (instrumento != null && InstrumentosSeleccionados.Contains(instrumento))
            {
                InstrumentosSeleccionados.Remove(instrumento);
            }
        }

        private void OnQuitarOrganizadorClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var organizador = button?.BindingContext as Manager;

            if (organizador != null && OrganizadoresSeleccionados.Contains(organizador))
            {
                OrganizadoresSeleccionados.Remove(organizador);
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nombreEventoEntry.Text) ||
                locacionPicker.SelectedItem == null)
            {
                await DisplayAlert("Error", "Por favor, completa todos los campos obligatorios.", "OK");
                return;
            }

            try
            {
                int managerActualId = await _databaseService.ObtenerManagerIdActualAsync();
                if (managerActualId == -1)
                {
                    await DisplayAlert("Error", "No se pudo obtener tu sesión actual.", "OK");
                    return;
                }

                // Asegurar que el manager actual esté incluido como organizador
                if (!OrganizadoresSeleccionados.Any(m => m.Id == managerActualId))
                {
                    var managerActual = await _databaseService.ObtenerManagerPorIdAsync(managerActualId);
                    if (managerActual != null)
                        OrganizadoresSeleccionados.Add(managerActual);
                }

                var locacionSeleccionada = (Locacion)locacionPicker.SelectedItem;

                var nuevoEvento = new Evento
                {
                    Nombre = nombreEventoEntry.Text.Trim(),
                    FechaEvento = fechaEventoPicker.Date,
                    FechaMontaje = fechaMontajePicker.Date,
                    LocacionId = locacionSeleccionada.Id
                };

                int eventoId = await _databaseService.GuardarEventoAsync(nuevoEvento, locacionSeleccionada.Id);

                foreach (var artista in ArtistasSeleccionados)
                {
                    await _databaseService.AgregarArtistaEventoAsync(eventoId, artista.Id);
                }

                foreach (var instrumento in InstrumentosSeleccionados)
                {
                    await _databaseService.AgregarInstrumentoEventoAsync(eventoId, instrumento.Id);
                }

                foreach (var manager in OrganizadoresSeleccionados)
                {
                    await _databaseService.AgregarManagerEventoAsync(eventoId, manager.Id);
                }

                await DisplayAlert("Éxito", "Evento creado correctamente.", "OK");
                await Shell.Current.GoToAsync($"///{nameof(HomeManagerPage)}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo guardar el evento: {ex.Message}", "OK");
            }
        }


        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            try
            {
                bool confirm = await DisplayAlert("Cancelar", "¿Deseas cancelar la creación del evento?", "Sí", "No");
                if (confirm)
                {
                    await Shell.Current.GoToAsync($"///{nameof(HomeManagerPage)}");

                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }
    }
}
