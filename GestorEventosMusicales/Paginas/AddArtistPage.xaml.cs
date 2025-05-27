using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using GestorEventosMusicales.Modelos;
using GestorEventosMusicales.Data;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;

namespace GestorEventosMusicales.Paginas
{
    public partial class AddArtistPage : ContentPage, IQueryAttributable
    {
        private byte[] imageBytes;
        private string imagePath;
        private DatabaseService _databaseService;
        private int artistaId;
        private int managerId;

        public ObservableCollection<Manager> ManagersAsociados { get; set; }

        public AddArtistPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            ManagersAsociados = new ObservableCollection<Manager>();
            ManagersCollection.ItemsSource = ManagersAsociados;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            managerId = query.ContainsKey("managerId") ? Convert.ToInt32(query["managerId"]) : 0;

            // Establecer el managerId en el converter para controlar visibilidad
            if (Resources["ManagerVisibilityConverter"] is Converters.ManagerIdToVisibilityConverter converter)
            {
                converter.ActualManagerId = managerId;
            }

            if (query.ContainsKey("id"))
            {
                artistaId = Convert.ToInt32(query["id"]);
                CargarArtista();
            }
            else
            {
                artistaId = 0;
                LimpiarCampos();
                Title = "Crear Artista";
                CargarManagerActual();
            }
        }


        private async void CargarManagerActual()
        {
            var manager = await _databaseService.ObtenerManagerPorIdAsync(managerId); // Este devuelve el Manager

            if (manager != null)
            {
                ManagersAsociados.Clear();
                ManagersAsociados.Add(manager);
            }
        }

        private async Task CargarArtista()
        {
            try
            {
                var artista = await _databaseService.ObtenerArtistaPorIdAsync(artistaId);

                if (artista != null)
                {
                    imageBytes = null;
                    imagenPreview.Source = null;

                    nombreEntry.Text = artista.Nombre;
                    bandaEntry.Text = artista.Banda;
                    nacionalidadEntry.Text = artista.Nacionalidad;
                    fechaNacimientoPicker.Date = artista.FechaNacimiento;

                    if (artista.Imagen != null)
                    {
                        imageBytes = artista.Imagen;
                        imagenPreview.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    }

                    // Limpiar lista antes de cargar
                    ManagersAsociados.Clear();

                    // Cargar el manager actual primero
                    var managerActual = await _databaseService.ObtenerManagerPorIdAsync(managerId);
                    if (managerActual != null)
                    {
                        ManagersAsociados.Add(managerActual);
                    }

                    // Cargar los demás managers asociados al artista
                    var managers = await _databaseService.ObtenerManagersPorArtistaIdAsync(artistaId);

                    foreach (var m in managers)
                    {
                        // Evitar duplicados: no agregar el manager actual dos veces
                        if (m.Id != managerId)
                        {
                            ManagersAsociados.Add(m);
                        }
                    }

                    Title = "Editar Artista";
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo cargar el artista: {ex.Message}", "OK");
            }
        }

        private void LimpiarCampos()
        {
            nombreEntry.Text = string.Empty;
            bandaEntry.Text = string.Empty;
            nacionalidadEntry.Text = string.Empty;
            fechaNacimientoPicker.Date = DateTime.Today;
            imagenPreview.Source = null;
            imageBytes = null;
            ManagersAsociados.Clear();
        }

        private async void OnSeleccionarImagenClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync();
            if (result != null)
            {
                using (var stream = await result.OpenReadAsync())
                {
                    imageBytes = new byte[stream.Length];
                    await stream.ReadAsync(imageBytes, 0, (int)stream.Length);
                }

                // Cargar imagen desde los bytes ya leídos
                imagenPreview.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            try
            {
                var artista = new Artista
                {
                    Id = artistaId,
                    Nombre = nombreEntry.Text,
                    Banda = bandaEntry.Text,
                    Nacionalidad = nacionalidadEntry.Text,
                    FechaNacimiento = fechaNacimientoPicker.Date,
                    Imagen = imageBytes
                };

                if (artistaId == 0)
                {
                    await _databaseService.InsertarArtistaAsync(artista, ManagersAsociados.ToList());
                    await DisplayAlert("Éxito", "El artista ha sido creado correctamente.", "OK");
                }
                else
                {
                    await _databaseService.ActualizarArtistaAsync(artista, ManagersAsociados.ToList());
                    await DisplayAlert("Éxito", "El artista ha sido actualizado correctamente.", "OK");
                }

                await Shell.Current.GoToAsync($"///{nameof(ViewEditArtistPage)}");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudo guardar el artista: {ex.Message}", "OK");
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"///{nameof(ViewEditArtistPage)}");
        }

        private async void OnAñadirManagerClicked(object sender, EventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }

        private void OnQuitarManagerClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var manager = button?.BindingContext as Manager;

            if (manager != null && ManagersAsociados.Contains(manager))
            {
                ManagersAsociados.Remove(manager);
            }
        }
    }
}
