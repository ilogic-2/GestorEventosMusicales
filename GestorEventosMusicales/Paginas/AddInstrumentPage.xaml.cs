using GestorEventosMusicales.Data;
using GestorEventosMusicales.Modelos;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace GestorEventosMusicales.Paginas
{
    public partial class AddInstrumentPage : ContentPage, IQueryAttributable
    {
        private DatabaseService dbService;
        private Instrumento instrumentoActual;
        private int managerIdActual;

        public AddInstrumentPage()
        {
            try
            {
                InitializeComponent();
                dbService = new DatabaseService();
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", $"Constructor error: {ex.Message}", "OK");
                });
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("Instrumento") && query["Instrumento"] is Instrumento instrumentoAEditar)
            {
                instrumentoActual = instrumentoAEditar;
                nombreEntry.Text = instrumentoActual.Nombre;
                cantidadEntry.Text = instrumentoActual.Cantidad.ToString();
                proveedorPicker.SelectedItem = instrumentoActual.Proveedor == "Artista" ? "Artista" : "Ninguno / Organización";
                Title = "Editar Instrumento";
            }
            else
            {
                instrumentoActual = null;
                nombreEntry.Text = string.Empty;
                cantidadEntry.Text = string.Empty;
                proveedorPicker.SelectedItem = "Ninguno / Organización";
                Title = "Crear Instrumento";
            }

            if (query.ContainsKey("ManagerId"))
            {
                managerIdActual = (int)query["ManagerId"];
            }
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();
        }

        private async void OnGuardarClicked(object sender, EventArgs e)
        {
            // Mueve la obtención del ManagerId justo antes de guardar
            managerIdActual = await dbService.ObtenerManagerIdActualAsync();

            try
            {
                if (string.IsNullOrEmpty(nombreEntry.Text))
                {
                    await DisplayAlert("Error", "El nombre del instrumento es obligatorio.", "OK");
                    return;
                }

                if (!int.TryParse(cantidadEntry.Text, out int cantidad) || cantidad <= 0)
                {
                    await DisplayAlert("Error", "La cantidad debe ser un número válido mayor que cero.", "OK");
                    return;
                }

                var proveedor = proveedorPicker.SelectedItem?.ToString() ?? "Ninguno / Organización";

                if (managerIdActual <= 0)
                {
                    await DisplayAlert("Error", "No se pudo obtener el ManagerId actual.", "OK");
                    return;
                }

                if (instrumentoActual == null)
                {
                    var nuevoInstrumento = new Instrumento
                    {
                        Nombre = nombreEntry.Text,
                        Cantidad = cantidad,
                        Proveedor = proveedor,
                        ManagerId = managerIdActual
                    };

                    await dbService.InsertarInstrumentoAsync(nuevoInstrumento);
                    await DisplayAlert("Éxito", "Instrumento añadido correctamente.", "OK");
                }
                else
                {
                    instrumentoActual.Nombre = nombreEntry.Text;
                    instrumentoActual.Cantidad = cantidad;
                    instrumentoActual.Proveedor = proveedor;
                    instrumentoActual.ManagerId = managerIdActual;

                    await dbService.ActualizarInstrumentoAsync(instrumentoActual);
                    await DisplayAlert("Éxito", "Instrumento actualizado correctamente.", "OK");
                }

                await Shell.Current.GoToAsync(nameof(ViewEditInstrumentPage));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Se produjo un error al guardar: {ex.Message}", "OK");
            }
        }

        private async void OnCancelarClicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.GoToAsync(nameof(ViewEditInstrumentPage));
            }
            catch (Exception ex)
            {
                await Logger.GuardarLogAsync($"Error al regresar a ViewEditInstrumentPage: {ex.Message} - {ex.StackTrace}");
            }
        }
    }
}
