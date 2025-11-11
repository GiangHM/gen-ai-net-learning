using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;

namespace functioncallingapi
{
    internal class LocalFunction
    {
        private static IHttpClientFactory? _httpClientFactory;
        
        internal LocalFunction(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Description("Get the Weather")]
        internal static async Task<string> Getweather(string city)
        {
            Console.WriteLine($"\tFunction Call - Returning weather info of the: {city}");

            if (_httpClientFactory == null)
            {
                return $"Unable to get weather for {city} - HttpClientFactory not initialized";
            }

            try
            {
                var client = _httpClientFactory.CreateClient("WeatherApi");
                var response = await client.GetAsync($"WeatherForecast/GetWeatherForecast?city={Uri.EscapeDataString(city)}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var temperatureC = JsonSerializer.Deserialize<int>(content);
                    
                    // Create a WeatherInfo object with the retrieved data
                    var weatherInfo = new WeatherInfo
                    {
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        TemperatureC = temperatureC,
                        Summary = $"The weather in {city} is currently {(temperatureC > 20 ? "warm" : "cool")}"
                    };

                    return weatherInfo.ToString();
                }
                else
                {
                    return $"Failed to get weather for {city}. Status code: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Error getting weather for {city}: {ex.Message}";
            }
        }
    }
}
