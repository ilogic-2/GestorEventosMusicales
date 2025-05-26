using System;
using Microsoft.Maui.Controls;
using GestorEventosMusicales.Modelos;
using GestorEventosMusicales.Data;

namespace GestorEventosMusicales.Paginas
{
    public partial class RegisterPage : ContentPage
    {
        private DatabaseService _databaseService;

        public RegisterPage()
        {
            _databaseService = new DatabaseService();
            InitializeComponent();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            string nombre = usernameEntry.Text?.Trim();
            string correo = emailEntry.Text?.Trim();
            string telefono = phoneEntry.Text?.Trim();
            string contrasena = passwordEntry.Text;
            string confirmarContrasena = confirmPasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(telefono) ||
                string.IsNullOrWhiteSpace(contrasena) ||
                string.IsNullOrWhiteSpace(confirmarContrasena))
            {
                errorLabel.Text = "Por favor, completa todos los campos.";
                errorLabel.IsVisible = true;
                return;
            }

            if (contrasena != confirmarContrasena)
            {
                errorLabel.Text = "Las contraseñas no coinciden.";
                errorLabel.IsVisible = true;
                return;
            }

            var nuevoManager = new Manager
            {
                Nombre = nombre,
                Correo = correo,
                Telefono = telefono,
                Contrasena = contrasena
            };

            try
            {
                int idManager = _databaseService.GuardarManager(nuevoManager);
                Preferences.Set("usuarioId", idManager);

                await DisplayAlert("Registro exitoso", $"Bienvenido, {nombre}", "Aceptar");
                await Shell.Current.GoToAsync("//LoginPage");
            }
            catch (Exception ex)
            {
                errorLabel.Text = "Error al registrar: " + ex.Message;
                errorLabel.IsVisible = true;
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

    }
}
