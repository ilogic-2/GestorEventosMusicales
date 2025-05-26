using GestorEventosMusicales.Paginas;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Controls.Xaml;

namespace GestorEventosMusicales
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            //registrando todas las rutas
            InitializeComponent();

            Routing.RegisterRoute(nameof(AddArtistPage), typeof(AddArtistPage));
            Routing.RegisterRoute(nameof(AddInstrumentPage), typeof(AddInstrumentPage));
            Routing.RegisterRoute(nameof(AddLocationPage), typeof(AddLocationPage));
            Routing.RegisterRoute(nameof(CambiarContrasenaPage), typeof(CambiarContrasenaPage));
            Routing.RegisterRoute(nameof(EditEventPage), typeof(EditEventPage));
            Routing.RegisterRoute(nameof(EditarPerfilPage), typeof(EditarPerfilPage));



            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(HomeManagerPage), typeof(HomeManagerPage));
            Routing.RegisterRoute(nameof(CreateEventPage), typeof(CreateEventPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(ViewEditEventPage), typeof(ViewEditEventPage));
            Routing.RegisterRoute(nameof(ViewEditArtistPage), typeof(ViewEditArtistPage));
            Routing.RegisterRoute(nameof(ViewEditLocationPage), typeof(ViewEditLocationPage));
            Routing.RegisterRoute(nameof(ViewEditInstrumentPage), typeof(ViewEditInstrumentPage));
            Routing.RegisterRoute(nameof(ViewProfilePage), typeof(ViewProfilePage));
        }
    }
}
