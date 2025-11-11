using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace practice_sk_plugins_foundrylocal
{
    public class FlightBookingPlugin
    {
        private const string FilePath = "flightsData.json";
        private List<FlightModel> flights;

        public FlightBookingPlugin()
        {
            // Load flights from the file
            flights = LoadFlightsFromFile();
        }

        // Create a plugin function with kernel function attributes
        [KernelFunction("search_flights")]
        [Description("Search for available flights based on destination and departure date in format YYYY-MM-DD")]
        public IEnumerable<FlightModel> SearchFlights(string destination, string departureDate)
        {
            var results = flights.Where(f => f.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)
                                          && f.DepartureDate.Equals(departureDate, StringComparison.OrdinalIgnoreCase)
                                          && !f.IsBooked).ToList();
            return results;
        }


        // Create a kernel function to book flights
        [KernelFunction("book_flight")]
        [Description("Books a flight based on the flight ID provided")]
        [return: Description("Booking confirmation message")]
        public string BookFlight(int flightId)
        {
            // Add logic to book a flight
            var flight = flights.FirstOrDefault(f => f.Id == flightId);
            if (flight == null)
            {
                return "Flight not found. Please provide a valid flight ID.";
            }

            if (flight.IsBooked)
            {
                return $"You've already booked this flight.";
            }

            flight.IsBooked = true;
            SaveFlightsToFile();

            return @$"Flight booked successfully! Airline: {flight.Airline}, 
                     Destination: {flight.Destination}, 
                     Departure: {flight.DepartureDate}, 
                     Price: ${flight.Price}.";
        }


        private void SaveFlightsToFile()
        {
            var json = JsonSerializer.Serialize(flights, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        private List<FlightModel> LoadFlightsFromFile()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<List<FlightModel>>(json)!;
            }

            throw new FileNotFoundException($"The file '{FilePath}' was not found. Please provide a valid flights.json file.");
        }
    }

    // Flight model
    public class FlightModel
    {
        public int Id { get; set; }
        public required string Airline { get; set; }
        public required string Destination { get; set; }
        public required string DepartureDate { get; set; }
        public decimal Price { get; set; }
        public bool IsBooked { get; set; } = false; // Added to track booking status
    }
}
