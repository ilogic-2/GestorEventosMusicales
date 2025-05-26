namespace GestorEventosMusicales
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // Usamos la SplashPage como pantalla inicial
            MainPage = new NavigationPage(new Paginas.SplashPage());
        }
    }
}