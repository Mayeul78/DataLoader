using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TestAlpha;


// Utilization Example
//Don't forget to save API Key in .env
class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            // Initialize the DataLoader
            DataLoader loader = new DataLoader();

            // Specify the symbols
            string fromSymbol = "USD";
            string toSymbol = "EUR";

            // Specify start and end dates
            string startDateInput = "2002-01-01";
            string endDateInput = "2020-01-01";

            // Parse the input dates
            DateTime startDate = DateTime.ParseExact(startDateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            DateTime endDate = DateTime.ParseExact(endDateInput, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            // Load the filtered data
            JObject timeSeriesData = await loader.LoadDataAsync(fromSymbol, toSymbol, startDate, endDate, "FX_DAILY", "full");

            // Debugging: Print the raw API response to verify data
            Console.WriteLine("Filtered Data:");
            if (timeSeriesData.Count > 0)
            {
                foreach (var entry in timeSeriesData)
                {
                    Console.WriteLine($"{entry.Key}: {entry.Value}");
                }

                // Save the filtered data to a CSV file named `fromSymbol-toSymbol.csv` in the temp folder
                loader.SaveDataToCsv(timeSeriesData, fromSymbol, toSymbol);
            }
            else
            {
                Console.WriteLine("No data available for the specified date range.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}