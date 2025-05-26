using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using GestorEventosMusicales.Data;

namespace GestorEventosMusicales.Paginas
{
    public partial class HomeManagerPage : ContentPage
    {
        private DatabaseService _databaseService = new DatabaseService();

        public HomeManagerPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string nombreUsuario = Preferences.Get("usuarioNombre", "Usuario");
            SaludoLabel.Text = $"¡Hola, {nombreUsuario}!";

            var eventos = await _databaseService.ObtenerProximosEventosConManagersAsync(DateTime.Now, 3);

            EventosStack.Children.Clear();

            if (eventos.Count == 0)
            {
                SinEventosLabel.IsVisible = true;
            }
            else
            {
                SinEventosLabel.IsVisible = false;

                foreach (var evento in eventos)
                {
                    var frame = new Frame
                    {
                        BackgroundColor = Colors.White,
                        CornerRadius = 8,
                        Padding = 12,
                        HasShadow = true,
                        Content = new VerticalStackLayout
                        {
                            Spacing = 4,
                            Children =
                            {
                                new Label { Text = evento.Nombre, FontSize = 16, TextColor = Colors.Black },
                                new Label { Text = $"Fecha: {evento.FechaEvento:dd/MM/yyyy}", FontSize = 14, TextColor = Colors.DarkGray },
                                new Label { Text = $"Lugar: {evento.Locacion?.Nombre ?? "No asignado"}", FontSize = 14, TextColor = Colors.DarkGray },
                                new Label {
                                    Text = "Managers: " + (evento.Managers.Count > 0
                                        ? string.Join(", ", evento.Managers.Select(m => m.Nombre))
                                        : "No asignados"),
                                    FontSize = 14,
                                    TextColor = Colors.DarkGray
                                }
                            }
                        }
                    };

                    EventosStack.Children.Add(frame);
                }
            }
        }

        private async void OnCrearEventoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(CreateEventPage));
        }

        private async void OnVerEditarEventoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ViewEditEventPage));
        }

        private async void OnVerEditarArtistaClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ViewEditArtistPage));
        }

        private async void OnVerEditarLugarClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ViewEditLocationPage));
        }

        private async void OnVerEditarInstrumentoClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ViewEditInstrumentPage));
        }

        private async void OnVerPerfilClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ViewProfilePage));
        }
    }
}
