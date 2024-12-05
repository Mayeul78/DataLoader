namespace TestAlpha;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;
using dotenv.net;

public class DataLoader
{
    private readonly string _apiKey;
    private readonly RestClient _client;
    private readonly string _tempFolderPath;

    public DataLoader()
    {
        // Load environment variables
        DotEnv.Load();

        // Get the API key
        _apiKey = Environment.GetEnvironmentVariable("ALPHA_VANTAGE_API_KEY");
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new Exception("API key is missing. Ensure it's set in the .env file.");
        }

        // Initialize the RestClient
        _client = new RestClient("https://www.alphavantage.co");

        // Initialize the temp folder path inside the project directory
        _tempFolderPath = Path.Combine(Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? ".", "temp");

        // Create the temp folder if it does not exist
        if (!Directory.Exists(_tempFolderPath))
        {
            Directory.CreateDirectory(_tempFolderPath);
        }
    }

    public async Task<JObject> LoadDataAsync(string fromSymbol, string toSymbol, DateTime startDate, DateTime endDate, string function = "FX_DAILY", string outputSize = "full")
    {
        // Build the API request
        var request = new RestRequest("/query", Method.Get);
        request.AddParameter("function", function);
        request.AddParameter("from_symbol", fromSymbol);
        request.AddParameter("to_symbol", toSymbol);
        request.AddParameter("apikey", _apiKey);
        request.AddParameter("outputsize", outputSize);

        // Execute the request
        var response = await _client.ExecuteAsync(request);

        if (response.IsSuccessful)
        {
            // Parse the response content into a JObject
            var data = JObject.Parse(response.Content);

            if (data.ContainsKey("Time Series FX (Daily)"))
            {
                var timeSeriesData = data["Time Series FX (Daily)"] as JObject;

                // Filter the time series data based on the date range
                var filteredData = new JObject();

                foreach (var property in timeSeriesData.Properties())
                {
                    DateTime entryDate = DateTime.Parse(property.Name);
                    if (entryDate >= startDate && entryDate <= endDate)
                    {
                        filteredData.Add(property.Name, property.Value);
                    }
                }

                return filteredData;
            }
            else
            {
                throw new Exception("Error: No 'Time Series FX (Daily)' data found in the response.");
            }
        }
        else
        {
            throw new Exception($"Error fetching data from Alpha Vantage: {response.StatusDescription}");
        }
    }

    public void SaveDataToCsv(JObject timeSeriesData, string fromSymbol, string toSymbol)
    {
        // Build the file name dynamically
        string fileName = $"{fromSymbol}-{toSymbol}.csv";
        string filePath = Path.Combine(_tempFolderPath, fileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Date,Open,High,Low,Close");

            foreach (var property in timeSeriesData.Properties())
            {
                string date = property.Name; // Get the date key
                var details = property.Value as JObject;
                string open = details?["1. open"]?.ToString() ?? "N/A";
                string high = details?["2. high"]?.ToString() ?? "N/A";
                string low = details?["3. low"]?.ToString() ?? "N/A";
                string close = details?["4. close"]?.ToString() ?? "N/A";

                writer.WriteLine($"{date},{open},{high},{low},{close}");
            }
        }

        Console.WriteLine($"Data successfully saved to: {filePath}");
    }
}
