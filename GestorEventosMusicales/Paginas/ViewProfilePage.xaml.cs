using System;
using GestorEventosMusicales.Data;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.IO;

namespace GestorEventosMusicales.Paginas
{
    public partial class ViewProfilePage : ContentPage
    {
        private DatabaseService _databaseService;

        public ViewProfilePage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            int id = Preferences.Get("usuarioId", 0);
            if (id == 0)
            {
                await Shell.Current.GoToAsync(nameof(LoginPage));
                return;
            }

            string nombreUsuario = Preferences.Get("usuarioNombre", "Usuario");
            string correoUsuario = Preferences.Get("usuarioCorreo", "correo@ejemplo.com");

            nombreLabel.Text = nombreUsuario;
            correoLabel.Text = correoUsuario;

            var manager = await _databaseService.ObtenerManagerPorIdAsync(id);
            if (manager?.Imagen != null)
            {
                imagen.Source = ImageSource.FromStream(() => new MemoryStream(manager.Imagen));
            }
            else
            {
                imagen.Source = "usuario.png";
            }
        }

        private async void OnEditarDatosClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(EditarPerfilPage));
        }

        private async void OnCambiarContrasenaClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CambiarContrasenaPage));
        }

        private async void OnCerrarSesionClicked(object sender, EventArgs e)
        {
            bool confirmacion = await DisplayAlert("Cerrar sesión", "¿Estás seguro de que deseas cerrar sesión?", "Sí", "Cancelar");

            if (confirmacion)
            {
                Preferences.Remove("usuarioId");
                Preferences.Remove("usuarioNombre");
                Preferences.Remove("usuarioCorreo");

                await Shell.Current.GoToAsync(nameof(LoginPage));
            }
        }
    }
}
