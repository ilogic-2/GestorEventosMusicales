using GestorEventosMusicales.Data;
using GestorEventosMusicales.Modelos;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;

namespace GestorEventosMusicales.Paginas
{
    public partial class ViewEditInstrumentPage : ContentPage
    {
        public ObservableCollection<Instrumento> Instrumentos { get; set; }
        private readonly DatabaseService _databaseService;

        public ViewEditInstrumentPage()
        {

            InitializeComponent();
            _databaseService = new DatabaseService();
            _ = CargarInstrumentosAsync();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = CargarInstrumentosAsync();
        }

        private async Task CargarInstrumentosAsync()
        {
            try
            {
                await Logger.GuardarLogAsync("Iniciando carga de instrumentos.");
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();
                await Logger.GuardarLogAsync($"ManagerId obtenido: {managerId}");

                var listaInstrumentos = _databaseService.ObtenerInstrumentos(managerId);
                Instrumentos = new ObservableCollection<Instrumento>(listaInstrumentos);
                instrumentList.ItemsSource = Instrumentos;

                await Logger.GuardarLogAsync("Instrumentos cargados correctamente.");
            }
            catch (Exception ex)
            {
                await Logger.GuardarLogAsync($"Error al cargar instrumentos: {ex.Message} - {ex.StackTrace}");
                await DisplayAlert("Error1", "No se ha encontrado el ID del manager. Inicia sesión nuevamente.", "OK");
            }
        }

        private async void OnAddInstrumentClicked(object sender, EventArgs e)
        {
            try
            {
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();

                var parametros = new Dictionary<string, object>
        {
            { "ManagerId", managerId }
        };

                await Shell.Current.GoToAsync(nameof(AddInstrumentPage), parametros);
            }
            catch (Exception ex)
            {
                await Logger.GuardarLogAsync($"Error al agregar instrumento: {ex.Message} - {ex.StackTrace}");
                await DisplayAlert("Error2", "No se ha encontrado el ID del manager. Inicia sesión nuevamente.", "OK");
            }
        }

        private async void OnEditInstrumentClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Instrumento instrumentoAEditar)
                return;

            try
            {

                int managerId = await _databaseService.ObtenerManagerIdActualAsync();

                var parametros = new Dictionary<string, object>
        {
            { "Instrumento", instrumentoAEditar },
            { "ManagerId", managerId }
        };

                await Shell.Current.GoToAsync(nameof(AddInstrumentPage), parametros);
            }
            catch (Exception ex)
            {
                // Log de error detallado
                await Logger.GuardarLogAsync($"Excepción al intentar editar el instrumento: {ex.Message} - {ex.StackTrace}");
                await DisplayAlert("Error", "Ha ocurrido un error. Verifica los logs para más detalles.", "OK");
            }
        }

        private async void OnDeleteInstrumentClicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.CommandParameter is not Instrumento instrumentoAEliminar)
                return;

            bool confirmacion = await DisplayAlert("Confirmar eliminación",
                                                   $"¿Seguro que quieres eliminar '{instrumentoAEliminar.Nombre}'?",
                                                   "Sí", "No");
            if (!confirmacion)
                return;

            try
            {
                await Logger.GuardarLogAsync($"Iniciando eliminación del instrumento: {instrumentoAEliminar.Nombre}. InstrumentoId: {instrumentoAEliminar.Id}");
                int managerId = await _databaseService.ObtenerManagerIdActualAsync();
                await Logger.GuardarLogAsync($"ManagerId obtenido para eliminar instrumento: {managerId}");

                await _databaseService.ActualizarInstrumentoAEventosNullAsync(instrumentoAEliminar.Id);
                await _databaseService.EliminarInstrumentoAsync(instrumentoAEliminar.Id, managerId);

                Instrumentos.Remove(instrumentoAEliminar);
                await Logger.GuardarLogAsync($"Instrumento eliminado correctamente: {instrumentoAEliminar.Nombre}. InstrumentoId: {instrumentoAEliminar.Id}");
                await DisplayAlert("Éxito", "Instrumento eliminado correctamente", "OK");
            }
            catch (Exception ex)
            {
                await Logger.GuardarLogAsync($"Error al eliminar instrumento: {ex.Message} - {ex.StackTrace}");
                await DisplayAlert("Error", $"No se pudo eliminar el instrumento: {ex.Message}", "OK");
            }
        }
    }
}
