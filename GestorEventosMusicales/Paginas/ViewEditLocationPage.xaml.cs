using GestorEventosMusicales.Data;
using GestorEventosMusicales.Modelos;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace GestorEventosMusicales.Paginas
{
    public partial class ViewEditLocationPage : ContentPage
    {
        public ObservableCollection<Locacion> Lugares { get; set; }
        private readonly DatabaseService _databaseService;

        public ViewEditLocationPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            _ = CargarLugaresAsync();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = this;
            await CargarLugaresAsync();
        }

        private async Task CargarLugaresAsync()
        {
            int managerId = await _databaseService.ObtenerManagerIdActualAsync();

            var manager = await _databaseService.ObtenerManagerPorIdAsync(managerId);

            bool esAdmin = manager?.Rol?.ToLower() == "admin";

            List<Locacion> listaLugares;

            if (esAdmin)
            {
                // Obtener todas las locaciones
                listaLugares = await _databaseService.ObtenerTodasLasLocacionesAsync();
            }
            else
            {
                // Obtener solo las locaciones vinculadas a este manager
                listaLugares = await _databaseService.ObtenerLocacionesAsync(managerId);
            }

            Lugares = new ObservableCollection<Locacion>(listaLugares);
            locationList.ItemsSource = Lugares;
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var texto = e.NewTextValue.ToLower();
            foreach (var lugar in Lugares)
            {
                lugar.IsVisible = lugar.Direccion.ToLower().Contains(texto);
            }

            locationList.ItemsSource = Lugares.Where(l => l.IsVisible).ToList();
        }

        private async void OnAddLocationClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AddLocationPage));
        }

        private async void OnEditarLocacionClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Locacion locacionAEditar)
            {
                var parametros = new Dictionary<string, object>
        {
            { "Locacion", locacionAEditar }
        };

                await Shell.Current.GoToAsync(nameof(AddLocationPage), parametros);
            }
        }

        private async void OnEliminarLocacionClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Locacion locacion)
            {
                bool confirmacion = await DisplayAlert("Confirmar", $"¿Eliminar el lugar \"{locacion.Nombre}\"?", "Sí", "No");

                if (confirmacion)
                {
                    int managerId = await _databaseService.ObtenerManagerIdActualAsync();
                    int resultado = await _databaseService.EliminarLocacionAsync(locacion.Id, managerId);

                    if (resultado > 0)
                    {
                        Lugares.Remove(locacion);
                        await DisplayAlert("Éxito", "Lugar eliminado correctamente.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo eliminar el lugar.", "OK");
                    }
                }
            }
        }

    }
}
