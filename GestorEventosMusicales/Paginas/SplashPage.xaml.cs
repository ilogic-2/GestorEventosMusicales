using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace GestorEventosMusicales.Paginas
{
    public partial class SplashPage : ContentPage
    {
        public SplashPage()
        {
            InitializeComponent();

            // Ejecutamos al cargar, usando Dispatcher para asegurar que se hace en el hilo principal
            Application.Current?.Dispatcher.Dispatch(async () =>
            {
                try
                {
                    await Task.Delay(500); // Asegura que el Shell ya esté montado

                    // Verificación de que Shell.Current no es null
                    if (Shell.Current == null)
                    {
                        Console.WriteLine("Shell.Current es null. Reiniciando MainPage...");
                        Application.Current.MainPage = new AppShell();
                        await Task.Delay(100);
                    }

                    if (Preferences.ContainsKey("usuarioId"))
                    {
                        var nombreUsuario = Preferences.Get("usuarioNombre", "");

                        if (!string.IsNullOrWhiteSpace(nombreUsuario))
                        {
                            Console.WriteLine("Redirigiendo a HomeManagerPage con nombreUsuario: " + nombreUsuario);
                            await Shell.Current.GoToAsync($"//HomeManagerPage?nombreUsuario={nombreUsuario}");
                        }
                        else
                        {
                            Console.WriteLine("Nombre de usuario vacío, limpiando preferencias.");
                            Preferences.Clear();
                            await Shell.Current.GoToAsync("//LoginPage", true);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No hay usuario guardado. Enviando a LoginPage.");
                        await Shell.Current.GoToAsync("//LoginPage", true);
                    }
                }
                catch (Exception ex)
                {
                    Preferences.Clear();
                    await DisplayAlert("Error", $"No se pudo: {ex.Message}", "OK");
                    Console.WriteLine("Error al iniciar sesión: " + ex.Message);
                    Application.Current.MainPage = new AppShell();
                    await Task.Delay(100);
                    await Shell.Current.GoToAsync("//LoginPage", true);
                }
            });
        }
    }
}
