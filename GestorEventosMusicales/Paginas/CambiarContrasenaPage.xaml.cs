using System;
using GestorEventosMusicales.Data;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace GestorEventosMusicales.Paginas
{
    public partial class CambiarContrasenaPage : ContentPage
    {
        private DatabaseService _db = new DatabaseService();

        public CambiarContrasenaPage()
        {
            InitializeComponent();
        }

        private async void OnCambiarClicked(object sender, EventArgs e)
        {
            int id = Preferences.Get("usuarioId", 0);
            string correo = Preferences.Get("usuarioCorreo", "");

            if (id == 0)
            {
                await DisplayAlert("Error", "No se encontró la sesión del usuario.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(actualEntry.Text) || string.IsNullOrWhiteSpace(nuevaEntry.Text))
            {
                await DisplayAlert("Error", "Todos los campos son obligatorios.", "OK");
                return;
            }

            if (nuevaEntry.Text != confirmarEntry.Text)
            {
                await DisplayAlert("Error", "La nueva contraseña no coincide.", "OK");
                return;
            }

            var valido = _db.ValidarCredenciales(correo, actualEntry.Text);
            if (valido == null)
            {
                await DisplayAlert("Error", "La contraseña actual es incorrecta.", "OK");
                return;
            }

            bool cambiado = _db.CambiarContrasena(id, nuevaEntry.Text);

            if (cambiado)
            {
                await DisplayAlert("Éxito", "Contraseña actualizada correctamente.", "OK");
                await Shell.Current.GoToAsync(nameof(ViewProfilePage));
            }
            else
            {
                await DisplayAlert("Error", "No se pudo cambiar la contraseña.", "OK");
            }
        }
        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ViewProfilePage));
        }
    }
}
