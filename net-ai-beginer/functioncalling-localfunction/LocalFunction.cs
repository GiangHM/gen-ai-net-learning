
using System.ComponentModel;

namespace functioncalling_localfunction
{
    internal class LocalFunction
    {
        [Description("Get the Weather")]
        internal static string Getweather()
        {
            var temperature = Random.Shared.Next(5, 20);
            var conditions = Random.Shared.Next(0, 1) == 0 ? "sunny" : "rainy";
            var weatherInfo = $"The weather is {temperature} degrees C and {conditions}.";
            Console.WriteLine($"\tFunction Call - Returning weather info: {weatherInfo}");
            return weatherInfo;
        }
    }
}
