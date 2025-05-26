using System;
using GestorEventosMusicales.Data;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using GestorEventosMusicales.Modelos;

namespace GestorEventosMusicales.Paginas
{
    public partial class LoginPage : ContentPage
    {
        private DatabaseService _databaseService;

        public LoginPage()
        {
            _databaseService = new DatabaseService();
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            string correo = usernameEntry.Text?.Trim();
            string contrasena = passwordEntry.Text;

            try
            {
                var manager = _databaseService.ValidarCredenciales(correo, contrasena);

                if (manager != null)
                {
                    // Guardar sesión con Preferences
                    Preferences.Set("usuarioId", manager.Id);
                    Preferences.Set("usuarioNombre", manager.Nombre);
                    Preferences.Set("usuarioCorreo", manager.Correo);

                    // Redirigir al HomeManagerPage
                    await Shell.Current.GoToAsync($"//HomeManagerPage?nombreUsuario={manager.Nombre}");
                }
                else
                {
                    errorLabel.Text = "Correo o contraseña incorrectos.";
                    errorLabel.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                errorLabel.Text = "Error al iniciar sesión: " + ex.Message;
                errorLabel.IsVisible = true;
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(RegisterPage));
        }
    }
}
