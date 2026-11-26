using System;
using System.Collections.Generic;
using System.Text;

namespace SistemaPatentesCIF.services
{
    using SistemaPatentesCIF.utils;
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public static class PlateLookupApi
    {
        // Se recomienda reutilizar la instancia de HttpClient en aplicaciones reales 
        // para evitar el agotamiento de sockets.
        private static readonly HttpClient client = new HttpClient();
        private static string? jsonResult;

        private const string apiKey = "clk_xDLme7-6YGoZr5knHsvcXSAojDNrwc3uLU5cF98Nmyw"; // Reemplazar con tu clave real

        public static async Task<string?> LookupPlate(string plate)
        {
            // 1. Reemplazá con tu clave generada en el panel de Clasificar (Llaves API)
            //string apiKey = "clk_xDLme7-6YGoZr5knHsvcXSAojDNrwc3uLU5cF98Nmyw";
            string patenteAConsultar = plate; // Formato de patente a buscar

            try
            {
                await ConsultarPatenteClasificarAsync(patenteAConsultar, apiKey);
                return jsonResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ocurrió un error: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> IsSiteAvailable()
        {
            try
            {
                using HttpClient client = new();

                client.Timeout =
                    TimeSpan.FromSeconds(5);

                HttpResponseMessage response =
                    await client.GetAsync(
                        "https://clasific.ar");

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task ConsultarPatenteClasificarAsync(string patente, string apiKey)
        {
            // Endpoint básico de identificación vehicular según la documentación de Clasificar
            string url = $"https://api.clasific.ar/v2/vehicles/{patente}?classification=true";

            // Crear el mensaje de la solicitud HTTP GET
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                // Inyectar el Header obligatorio de autenticación de Clasificar
                request.Headers.Add("x-api-key", apiKey);
                request.Headers.Add("Accept", "application/json");

                // Enviar la petición de forma asincrónica
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Respuesta exitosa de Clasificar:");
                    // Parsear los datos del JSON recibido
                    Debug.WriteLine(jsonResponse);
                    jsonResult = jsonResponse;
                }
                else
                {
                    jsonResult = null;
                }
            }
        }
    }
}