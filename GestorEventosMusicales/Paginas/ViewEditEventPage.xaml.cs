using System.Collections.ObjectModel;
using GestorEventosMusicales.Data;
using GestorEventosMusicales.Modelos;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace GestorEventosMusicales.Paginas
{
    public partial class ViewEditEventPage : ContentPage
    {
        private ObservableCollection<Evento> Eventos { get; set; }
        private ObservableCollection<Evento> EventosOriginales { get; set; }
        private readonly DatabaseService _databaseService;

        public ViewEditEventPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            Eventos = new ObservableCollection<Evento>();
            EventosOriginales = new ObservableCollection<Evento>();
            eventosList.ItemsSource = Eventos;
            searchBar.TextChanged += OnSearchTextChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CargarEventosAsync();
        }

        private async Task CargarEventosAsync()
        {
            try
            {
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();
                if (managerId == -1)
                {
                    await DisplayAlert("Error", "No se encontró el ID del manager", "OK");
                    return;
                }

                var manager = await _databaseService.ObtenerManagerPorIdAsync(managerId);
                bool esAdmin = manager?.Rol?.ToLower() == "admin";

                List<Evento> eventos;

                if (esAdmin)
                {
                    // El admin ve todos los eventos
                    eventos = await _databaseService.ObtenerEventosAsync();
                }
                else
                {
                    // Los demás managers solo los eventos donde están asignados
                    eventos = new List<Evento>();

                    var todosEventos = await _databaseService.ObtenerEventosAsync();

                    foreach (var evento in todosEventos)
                    {
                        evento.Managers = await _databaseService.ObtenerManagersPorEventoAsync(evento.Id);

                        if (evento.Managers.Any(m => m.Id == managerId))
                        {
                            eventos.Add(evento);
                        }
                    }
                }

                Eventos.Clear();
                EventosOriginales.Clear();

                foreach (var evento in eventos)
                {
                    if (evento.Managers == null)
                        evento.Managers = await _databaseService.ObtenerManagersPorEventoAsync(evento.Id);

                    Eventos.Add(evento);
                    EventosOriginales.Add(evento);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar eventos: {ex.Message}", "OK");
            }
        }


        // Método para filtrar la lista según el texto de búsqueda
        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = e.NewTextValue?.Trim().ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(texto))
            {
                // Si no hay texto, mostrar la lista original completa
                Eventos.Clear();
                foreach (var evento in EventosOriginales)
                    Eventos.Add(evento);
            }
            else
            {
                // Filtrar por nombre del evento o nombre de la locación
                var filtrados = EventosOriginales.Where(ev =>
                    (ev.Nombre?.ToLower().Contains(texto) ?? false) ||
                    (ev.Locacion?.Nombre?.ToLower().Contains(texto) ?? false) ||
                    (ev.Locacion?.Direccion?.ToLower().Contains(texto) ?? false)
                ).ToList();

                Eventos.Clear();
                foreach (var ev in filtrados)
                    Eventos.Add(ev);
            }
        }

        private async void OnEditEventClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var evento = (Evento)button.BindingContext;

            var parametros = new Dictionary<string, object>
            {
                { "id", evento.Id }
            };
            await Shell.Current.GoToAsync(nameof(EditEventPage), parametros);
        }

        private async void OnDeleteEventClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Evento eventoAEliminar)
                return;

            bool confirmacion = await DisplayAlert("Eliminar Evento",
                $"¿Estás seguro de que quieres eliminar el evento '{eventoAEliminar.Nombre}'?", "Sí", "No");

            if (confirmacion)
            {
                try
                {
                    // Obtener el managerId usando el método ObtenerManagerIdActualAsync
                    int managerId = await _databaseService.ObtenerManagerIdActualAsync();

                    int resultado = await _databaseService.EliminarEventoAsync(eventoAEliminar.Id, managerId);

                    if (resultado == 1)
                    {
                        Eventos.Remove(eventoAEliminar);
                        await DisplayAlert("Éxito", $"El evento '{eventoAEliminar.Nombre}' ha sido eliminado correctamente.", "OK");
                    }
                    else if (resultado == 2)
                    {
                        Eventos.Remove(eventoAEliminar);
                        await DisplayAlert("Relación eliminada", $"El evento '{eventoAEliminar.Nombre}' ya no está vinculado contigo, pero sigue existiendo.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo eliminar el evento de la base de datos.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo eliminar el evento. Detalles: {ex.Message}", "OK");
                }
            }
        }
    }
}