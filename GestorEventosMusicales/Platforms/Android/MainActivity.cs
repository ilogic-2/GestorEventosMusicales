using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Microsoft.Maui.Controls;

namespace GestorEventosMusicales
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                               ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back)
            {
                _ = HandleBackPressedAsync();
                return true;
            }

            return base.OnKeyDown(keyCode, e);
        }

        private async Task HandleBackPressedAsync()
        {
            var shell = Shell.Current;
            var currentPage = shell?.CurrentPage;

            if (currentPage == null)
            {
                FinishAffinity();
                return;
            }

            if (currentPage is GestorEventosMusicales.Paginas.LoginPage ||
                currentPage is GestorEventosMusicales.Paginas.HomeManagerPage)
            {
                FinishAffinity();
                return;
            }

            if (currentPage is GestorEventosMusicales.Paginas.RegisterPage)
            {
                await shell.GoToAsync("///LoginPage");
                return;
            }

            // Mapear páginas terciarias a sus páginas secundarias explícitas
            var terciariasConPadre = new Dictionary<Type, string>
    {
        { typeof(GestorEventosMusicales.Paginas.EditEventPage), "ViewEditEventPage" },
        { typeof(GestorEventosMusicales.Paginas.AddArtistPage), "ViewEditArtistPage" },
        { typeof(GestorEventosMusicales.Paginas.AddLocationPage), "ViewEditLocationPage" },
        { typeof(GestorEventosMusicales.Paginas.AddInstrumentPage), "ViewEditInstrumentPage" },
        { typeof(GestorEventosMusicales.Paginas.CambiarContrasenaPage), "ViewProfilePage" },
        { typeof(GestorEventosMusicales.Paginas.EditarPerfilPage), "ViewProfilePage" }
    };

            var tipoActual = currentPage.GetType();

            if (terciariasConPadre.ContainsKey(tipoActual))
            {
                var paginaPadreRoute = terciariasConPadre[tipoActual];
                // Navegar explícito a la página padre
                await shell.GoToAsync($"///{paginaPadreRoute}");
            }
            else
            {
                var paginasSecundarias = new Type[]
                {
            typeof(GestorEventosMusicales.Paginas.CreateEventPage),
            typeof(GestorEventosMusicales.Paginas.ViewEditInstrumentPage),
            typeof(GestorEventosMusicales.Paginas.ViewEditArtistPage),
            typeof(GestorEventosMusicales.Paginas.ViewEditEventPage),
            typeof(GestorEventosMusicales.Paginas.ViewEditLocationPage),
            typeof(GestorEventosMusicales.Paginas.ViewProfilePage)
                };

                if (paginasSecundarias.Contains(tipoActual))
                {
                    await shell.GoToAsync("///HomeManagerPage");
                }
                else
                {
                    FinishAffinity();
                }
            }
        }

    }
}
