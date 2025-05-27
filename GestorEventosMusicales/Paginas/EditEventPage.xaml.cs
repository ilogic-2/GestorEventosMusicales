using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using GestorEventosMusicales.Modelos;
using GestorEventosMusicales.Data;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;

namespace GestorEventosMusicales.Paginas
{
    public partial class EditEventPage : ContentPage, IQueryAttributable
    {
        private int eventoId;
        private List<Locacion> locacionesDisponibles;

        private DatabaseService _databaseService;
        public Evento Evento { get; set; }

        public ObservableCollection<Artista> ArtistasAsociados { get; set; }
        public ObservableCollection<Instrumento> InstrumentosAsociados { get; set; }
        public ObservableCollection<Manager> ManagersAsociados { get; set; }

        public EditEventPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();

            ArtistasAsociados = new ObservableCollection<Artista>();
            artistasCollection.ItemsSource = ArtistasAsociados;

            InstrumentosAsociados = new ObservableCollection<Instrumento>();
            instrumentosCollection.ItemsSource = InstrumentosAsociados;

            ManagersAsociados = new ObservableCollection<Manager>();
            ManagersCollection.ItemsSource = ManagersAsociados;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("id"))
            {
                eventoId = Convert.ToInt32(query["id"]);
            }
            else
            {
                eventoId = 0;
                LimpiarCampos();
                Title = "Crear Evento";
            }

            // Establecer el managerId en el converter para controlar visibilidad
            if (Resources["ManagerVisibilityConverter"] is Converters.ManagerIdToVisibilityConverter converter)
            {
                _ = SetManagerIdInConverter(converter); // llamamos al método asíncrono
            }

            // Cargar el evento si corresponde
            if (eventoId != 0)
                _ = CargarEvento();
        }

        private async Task SetManagerIdInConverter(Converters.ManagerIdToVisibilityConverter converter)
        {
            int managerId = await _databaseService.ObtenerManagerIdActualAsync();
            converter.ActualManagerId = managerId;
        }

        // Método para cargar el evento desde la base de datos
        private async Task CargarEvento()
        {
            try
            {
                var evento = await _databaseService.ObtenerEventoPorIdAsync(eventoId);
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();


                if (evento != null)
                {
                    // Cargar la lista de locaciones antes de asignar la seleccionada
                    locacionesDisponibles = await _databaseService.ObtenerLocacionesParaEdicionAsync(eventoId, managerId);
                    locacionPicker.ItemsSource = locacionesDisponibles;
                    locacionPicker.ItemDisplayBinding = new Binding("Nombre");

                    nombreEntry.Text = evento.Nombre;
                    fechaEventoPicker.Date = evento.FechaEvento;
                    fechaMontajePicker.Date = evento.FechaMontaje;

                    // Asignar locación seleccionada si existe
                    var locacion = locacionesDisponibles.FirstOrDefault(l => l.Id == evento.LocacionId);
                    locacionPicker.SelectedItem = locacion;

                    // Mostrar el nombre y dirección de la locación seleccionada
                    locacionNombreLabel.Text = locacion?.Nombre ?? "Lugar no asignado";
                    locacionDireccionLabel.Text = locacion != null ? " --- " + locacion.Direccion : "Lugar no asignado";

                    // Cargar los artistas asociados
                    var artistas = await _databaseService.ObtenerArtistasPorEventoIdAsync(eventoId);
                    ArtistasAsociados.Clear();
                    foreach (var a in artistas)
                        ArtistasAsociados.Add(a);

                    // Cargar los instrumentos asociados
                    var instrumentos = await _databaseService.ObtenerInstrumentosPorEventoIdAsync(eventoId);
                    InstrumentosAsociados.Clear();
                    foreach (var i in instrumentos)
                        InstrumentosAsociados.Add(i);

                    // Cargar los managers asociados
                    var managers = await _databaseService.ObtenerManagersPorEventoIdAsync(eventoId);
                    ManagersAsociados.Clear();
                    foreach (var m in managers)
                        ManagersAsociados.Add(m);

                    Title = "Editar Evento";
                }
            }
            catch (Exception ex)
            {

            }
        }

        // Limpiar los campos para crear un nuevo evento
        private void LimpiarCampos()
        {
            nombreEntry.Text = string.Empty;
            fechaEventoPicker.Date = DateTime.Today;
            fechaMontajePicker.Date = DateTime.Today;
            locacionPicker.SelectedItem = null; // Limpiar la locación seleccionada
            ArtistasAsociados.Clear();
            InstrumentosAsociados.Clear();
            ManagersAsociados.Clear();
        }

        // Método para guardar los cambios del evento
        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            try
            {
                var evento = new Evento
                {
                    Id = eventoId,
                    Nombre = nombreEntry.Text,
                    FechaEvento = fechaEventoPicker.Date,
                    FechaMontaje = fechaMontajePicker.Date,
                    // Aquí revisamos si no hay selección de locación
                    LocacionId = (locacionPicker.SelectedItem as Locacion)?.Id ?? 0
                };

                // Actualizar evento
                await _databaseService.ActualizarEventoAsync(evento);

                // Actualizar relaciones
                await _databaseService.ActualizarArtistasDeEventoAsync(evento.Id, ArtistasAsociados.Select(a => a.Id).ToList());
                await _databaseService.ActualizarInstrumentosDeEventoAsync(evento.Id, InstrumentosAsociados.Select(i => i.Id).ToList());
                await _databaseService.ActualizarManagersDeEventoAsync(evento.Id, ManagersAsociados.Select(m => m.Id).ToList());

                await DisplayAlert("Éxito", "Evento actualizado correctamente", "OK");
                await Shell.Current.GoToAsync(nameof(ViewEditEventPage));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo guardar el evento: {ex.Message}", "OK");
            }
        }


        // Método para añadir un manager al evento
        private async void OnAñadirManagerClicked(object sender, EventArgs e)
        {
            var managersDisponibles = await _databaseService.ObtenerManagersAsync();

            var managersNoAsociados = managersDisponibles
                .Where(m => !ManagersAsociados.Any(ma => ma.Id == m.Id))
                .ToList();

            string[] nombres = managersNoAsociados.Select(m => m.Nombre).ToArray();

            string seleccionado = await DisplayActionSheet("Selecciona un Manager", "Cancelar", null, nombres);

            if (seleccionado != null && seleccionado != "Cancelar")
            {
                var manager = managersNoAsociados.FirstOrDefault(m => m.Nombre == seleccionado);
                if (manager != null)
                {
                    ManagersAsociados.Add(manager);
                }
            }
        }

        // Método para quitar un manager del evento
        private void OnQuitarManagerClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var manager = button?.BindingContext as Manager;

            if (manager != null && ManagersAsociados.Contains(manager))
            {
                ManagersAsociados.Remove(manager);
            }
        }

        // Método para añadir un artista al evento
        private async void OnAgregarArtistaClicked(object sender, EventArgs e)
        {
            int managerId = await _databaseService.ObtenerManagerIdActualAsync();
            var artistasDisponibles = await _databaseService.ObtenerArtistasAsync(managerId);

            var artistasNoAsociados = artistasDisponibles
                .Where(a => !ArtistasAsociados.Any(aa => aa.Id == a.Id))
                .ToList();

            string[] nombres = artistasNoAsociados.Select(a => a.Nombre).ToArray();

            string seleccionado = await DisplayActionSheet("Selecciona un Artista", "Cancelar", null, nombres);

            if (seleccionado != null && seleccionado != "Cancelar")
            {
                var artista = artistasNoAsociados.FirstOrDefault(a => a.Nombre == seleccionado);
                if (artista != null)
                {
                    ArtistasAsociados.Add(artista);
                }
            }
            
        }

        // Método para quitar un artista del evento
        private void OnQuitarArtistaClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var artista = button?.BindingContext as Artista;

            if (artista != null && ArtistasAsociados.Contains(artista))
            {
                ArtistasAsociados.Remove(artista);
            }
        }

        // Método para añadir un instrumento al evento
        private async void OnAgregarInstrumentoClicked(object sender, EventArgs e)
        {
            int managerId = await _databaseService.ObtenerManagerIdActualAsync();
            var instrumentosDisponibles = await _databaseService.ObtenerInstrumentosAsync(managerId);

            var instrumentosNoAsociados = instrumentosDisponibles
                .Where(i => !InstrumentosAsociados.Any(ii => ii.Id == i.Id))
                .ToList();

            string[] nombres = instrumentosNoAsociados.Select(i => i.Nombre).ToArray();

            string seleccionado = await DisplayActionSheet("Selecciona un Instrumento", "Cancelar", null, nombres);

            if (seleccionado != null && seleccionado != "Cancelar")
            {
                var instrumento = instrumentosNoAsociados.FirstOrDefault(i => i.Nombre == seleccionado);
                if (instrumento != null)
                {
                    InstrumentosAsociados.Add(instrumento);
                }
            }
        }

        // Método para quitar un instrumento del evento
        private void OnQuitarInstrumentoClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var instrumento = button?.BindingContext as Instrumento;

            if (instrumento != null && InstrumentosAsociados.Contains(instrumento))
            {
                InstrumentosAsociados.Remove(instrumento);
            }
        }

        // Método para cancelar la edición y volver atrás
        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ViewEditEventPage));
        }
    }
}