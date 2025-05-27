using System.Collections.ObjectModel;
using GestorEventosMusicales.Data;
using GestorEventosMusicales.Modelos;
using Microsoft.Maui.Storage;
using System.Linq;
using System.Threading.Tasks;

namespace GestorEventosMusicales.Paginas
{
    public partial class ViewEditArtistPage : ContentPage
    {
        public ObservableCollection<Artista> Artistas { get; set; }
        private List<Artista> ArtistasTotales = new();
        private readonly DatabaseService _databaseService;

        public ViewEditArtistPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _ = CargarArtistasAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = CargarArtistasAsync();
        }

        private async Task CargarArtistasAsync()
        {
            try
            {
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();

                if (managerId == -1)
                {
                    await DisplayAlert("Error", "No se ha encontrado el ID del manager. Inicia sesión nuevamente.", "OK");
                    return;
                }

                var manager = await _databaseService.ObtenerManagerPorIdAsync(managerId);
                bool esAdmin = manager?.Rol?.ToLower() == "admin";

                // Obtener todos los artistas si es admin, o solo los suyos si no lo es
                List<Artista> artistas;
                if (esAdmin)
                {
                    artistas = await _databaseService.ObtenerTodosLosArtistasAsync();
                }
                else
                {
                    artistas = await _databaseService.ObtenerArtistasAsync(managerId);
                }

                // Modificar la UI en el hilo principal
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Artistas = new ObservableCollection<Artista>();

                    foreach (var artista in artistas)
                    {
                        if (artista.Imagen != null)
                        {
                            artista.ImagenPreview = ImageSource.FromStream(() => new MemoryStream(artista.Imagen));
                        }

                        Artistas.Add(artista);
                    }

                    artistList.ItemsSource = Artistas;
                });
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo cargar los artistas: {ex.Message}", "OK");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = e.NewTextValue.ToLower();
            var filtrados = ArtistasTotales.Where(a =>
                a.Nombre.ToLower().Contains(texto) ||
                a.Banda.ToLower().Contains(texto) ||
                a.Nacionalidad.ToLower().Contains(texto)).ToList();

            Artistas.Clear();
            foreach (var artista in filtrados)
            {
                Artistas.Add(artista);
            }
        }

        // Método para navegar a la página de añadir un nuevo artista
        private async void OnAddArtistClicked(object sender, EventArgs e)
        {
            try
            {
                // Obtener el managerId usando el método ObtenerManagerIdActualAsync
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();

                var parametros = new Dictionary<string, object>
                {
                    { "ManagerId", managerId }
                };

                // Navegar a la página de agregar un nuevo artista, pasando el managerId
                await Shell.Current.GoToAsync(nameof(AddArtistPage), parametros);

            }
            catch (Exception)
            {
                await DisplayAlert("Error", "No se ha encontrado el ID del manager. Inicia sesión nuevamente.", "OK");
            }
        }

        // Método para editar un artista
        private async void OnEditArtistClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Artista artistaAEditar)
                return;

            try
            {
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();
                var parametros = new Dictionary<string, object>
        {
            { "id", artistaAEditar.Id },
            { "managerId", managerId }
        };
                await Shell.Current.GoToAsync(nameof(AddArtistPage), parametros);
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "No se ha encontrado el ID del manager. Inicia sesión nuevamente.", "OK");
            }
        }


        private async void OnDeleteArtistClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Artista artistaAEliminar)
                return;

            bool confirmacion = await DisplayAlert("Eliminar Artista",
                $"¿Estás seguro de que quieres eliminar a {artistaAEliminar.Nombre}?", "Sí", "No");

            if (confirmacion)
            {
                try
                {
                    // Obtener el managerId usando el método ObtenerManagerIdActualAsync
                    int managerId = await _databaseService.ObtenerManagerIdActualAsync();

                    int resultado = await _databaseService.EliminarArtistaAsync(artistaAEliminar.Id, managerId);

                    if (resultado == 1)
                    {
                        Artistas.Remove(artistaAEliminar);
                        await DisplayAlert("Éxito", $"{artistaAEliminar.Nombre} ha sido eliminado correctamente.", "OK");
                    }
                    else if (resultado == 2)
                    {
                        Artistas.Remove(artistaAEliminar);
                        await DisplayAlert("Relación eliminada", $"{artistaAEliminar.Nombre} ya no está vinculado contigo, pero sigue existiendo.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo eliminar el artista de la base de datos.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    // Mostrar más detalles sobre el error
                    await DisplayAlert("Error", $"No se pudo eliminar el artista. Detalles: {ex.Message}", "OK");
                }
            }
        }

    }
}
