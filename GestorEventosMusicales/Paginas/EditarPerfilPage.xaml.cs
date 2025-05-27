using System;
using GestorEventosMusicales.Data;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace GestorEventosMusicales.Paginas
{
    public partial class EditarPerfilPage : ContentPage
    {
        private DatabaseService _db = new DatabaseService();
        private byte[] _imagenSeleccionada;
        private byte[] _imagenActual;

        public EditarPerfilPage()
        {
            InitializeComponent();

            nombreEntry.Text = Preferences.Get("usuarioNombre", "");
            correoEntry.Text = Preferences.Get("usuarioCorreo", "");
            telefonoEntry.Text = Preferences.Get("usuarioTelefono", "");

            // Cargar imagen actual (si existe)
            CargarImagenPerfil();
        }

        private async void OnSeleccionarImagenClicked(object sender, EventArgs e)
        {
            var resultado = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Selecciona una imagen de perfil"
            });

            if (resultado != null)
            {
                using var stream = await resultado.OpenReadAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                _imagenSeleccionada = ms.ToArray();

                // Usar una copia del stream para evitar errores
                if (_imagenSeleccionada != null)
                {
                    var streamImagen = new MemoryStream(_imagenSeleccionada);
                    imagenPerfil.Source = ImageSource.FromStream(() => streamImagen);
                }
            }
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            int id = Preferences.Get("usuarioId", 0);

            if (id == 0)
            {
                await DisplayAlert("Error", "No se pudo obtener el ID del usuario.", "OK");
                return;
            }

            // Si no se seleccionó una imagen nueva, mantener la imagen actual
            byte[] imagenParaGuardar = _imagenSeleccionada ?? _imagenActual;

            bool resultado = _db.ActualizarDatosManager(id, nombreEntry.Text, correoEntry.Text, telefonoEntry.Text, imagenParaGuardar);

            if (resultado)
            {
                Preferences.Set("usuarioNombre", nombreEntry.Text);
                Preferences.Set("usuarioCorreo", correoEntry.Text);
                Preferences.Set("usuarioTelefono", telefonoEntry.Text);

                await DisplayAlert("Éxito", "Datos actualizados correctamente.", "OK");
                await Shell.Current.GoToAsync(nameof(ViewProfilePage));
            }
            else
            {
                await DisplayAlert("Error", "No se pudo actualizar el perfil.", "OK");
            }
        }

        private async void CargarImagenPerfil()
        {
            int id = Preferences.Get("usuarioId", 0);
            var manager = await _db.ObtenerManagerPorIdAsync(id);

            if (manager?.Imagen != null)
            {
                _imagenActual = manager.Imagen;
                var streamImagen = new MemoryStream(manager.Imagen);
                imagenPerfil.Source = ImageSource.FromStream(() => streamImagen);
            }
            else
            {
                _imagenActual = null;
                imagenPerfil.Source = "default_image.png"; // Imagen por defecto si no hay imagen
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ViewProfilePage));
        }
    }
}
