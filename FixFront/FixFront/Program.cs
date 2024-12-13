using FixFront.Models;
using FixFront.Services;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

class Program
{
    private static HubConnection connection;
    private static InfluxDBService _service = new InfluxDBService();
    //private const string _baseUrl = "http://13.93.28.29:84";
    //private const string _baseUrl = "http://20.67.34.118:88";
    private const string _baseUrl = "https://localhost:7261";
    //private const string _baseUrl = "http://20.67.34.118:88";

    static async Task Main(string[] args)
    {
        //await Task.Delay(TimeSpan.FromSeconds(5));

        Console.WriteLine("Login");

        var token = await AuthenticateAsync($"{_baseUrl}/api/Auth/Login", "user@example.com", "Pass!123");
        //var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6InVzZXJAZXhhbXBsZS5jb20iLCJyb2xlIjoidXNlciIsIm5iZiI6MTczNDAwOTg5NiwiZXhwIjoxNzM0MDk2Mjk2LCJpYXQiOjE3MzQwMDk4OTYsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjcyNjEiLCJhdWQiOiJodHRwOi8vbG9jYWxob3N0OjMwMDAifQ.-MSxfBAnu27rxNbnRAaKLHALn8pyl64Dn6FMzFZlepg";
        if (token != null)
        {
            Console.WriteLine("Login successful! Token stored.");
        }
        else
        {
            Console.WriteLine("Login failed.");
            //return;
        }

        connection = new HubConnectionBuilder()
            .WithUrl($"{_baseUrl}/trade", opt =>
            {
                opt.AccessTokenProvider = () => Task.FromResult(token);
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On("Pong", (string data) =>
        {
            Console.WriteLine($"Ping reply => {data}");
        });

        connection.On("Connected", (string data) =>
        {
            Console.WriteLine($"Connect Centroid => {data}");
        });

        connection.On("SymbolQuotes", (object data) =>
        {
            Console.WriteLine($"Quotes => {data}");
        });

        await connection.StartAsync();
        Console.WriteLine("Connected to the Trade Hub");

        await connection.InvokeAsync("Ping");
        //return;
        await connection.InvokeAsync("ConnectCentroid", token);

        var newOrder1 = new CreateOrderModel
        {
            Type = "market",
            ClOrdId = DateTime.UtcNow.ToLongDateString(),
            CloseTime = DateTime.UtcNow,
            Status = "IDK",
            GatewayType = "BBook",
            SymbolId = 1,
            SymbolName = "EURUSD",
            TradeSide = "buy",
            Quantity = 10,
            RiskUserId = 1
        };
        var newOrder2 = new CreateOrderModel
        {
            Type = "market",
            ClOrdId = DateTime.UtcNow.ToLongDateString(),
            CloseTime = DateTime.UtcNow,
            Status = "IDK",
            GatewayType = "BBook",
            SymbolId = 2,
            SymbolName = "GBPUSD",
            TradeSide = "buy",
            Quantity = 10,
            RiskUserId = 1
        };

        await PostHttp($"{_baseUrl}/api/Orders", token, newOrder1);
        await PostHttp($"{_baseUrl}/api/Orders", token, newOrder1);
        await PostHttp($"{_baseUrl}/api/Orders", token, newOrder2);

        var cts = new CancellationTokenSource();

        //await FetchQuotes(connection, token, cts.Token);
        //await FetchPositions(connection, token, cts.Token);
        await FetchUsers(connection, token, cts.Token);

        Console.WriteLine("Enter trade info to send (or 'exit' to quit):");
        Console.ReadLine();

        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    static async Task FetchPositions(HubConnection connection, string token, CancellationToken cancellationToken)
    {
        var positions = connection.StreamAsync<Position>("StreamBPositions", token, cancellationToken);

        Console.WriteLine("Fetching Positions!");

        await foreach (var position in positions)
        {
            Console.WriteLine($"Position:{position.SymbolName} EntryPrice:{position.EntryPrice}" +
                $" TradeSide:{position.TradeSide} UserID:{position.RiskUserId} PnL:{position.Profit}");
        }
    }

    static async Task FetchUsers(HubConnection connection, string token, CancellationToken cancellationToken)
    {
        var users = connection.StreamAsync<UserMargin>("StreamMargin", token, cancellationToken);

        await foreach (var user in users)
        {
            Console.WriteLine($"UserID:{user.RiskUserId}");
        }
    }

    static async Task FetchQuotes(HubConnection connection, string token, CancellationToken cancellationToken)
    {
        var quotes = connection.StreamAsync<SymbolQuote>("SymbolQuotes", token, cancellationToken);

        Console.WriteLine("Fetching Quotes!");

        await foreach (var quote in quotes)
        {

            Console.WriteLine($"Quote Symbol:{quote.SymbolName} Bid:{quote.Bid} Ask:{quote.Ask}");

            //_service.Write(write =>
            //{
            //    DateTime original = DateTime.Now; // Current date and time

            //    // Round down to the nearest 10 minutes
            //    int minutes = original.Minute / 10 * 10;
            //    DateTime truncated = new DateTime(
            //        original.Year,
            //        original.Month,
            //        original.Day,
            //        original.Hour,
            //        original.Minute,   // Use the rounded minute value
            //        0          // Set seconds to 0
            //    );

            //    //var point = PointData.Measurement($"Symbols-{truncated}")
            //    var point = PointData.Measurement($"Symbols-{truncated}")
            //        .Tag("Id", quote.SymbolId.ToString())
            //        .Field("symbolObject", JsonSerializer.Serialize(quote))
            //        //.Field("BId", symbol.Bid)
            //        //.Field("ASK", symbol.Ask)
            //        //.Field("Digits", symbol.Digits)
            //        .Timestamp(DateTime.Now, WritePrecision.Ms);

            //    write.WritePoint(point, "Symbols", "organization");

            //});
        }
    }

    public record NewOrderRequestParameters(string Type, string ClOrdId, int? SymbolId, string? SymbolName, string TradeSide, decimal Quantity, decimal TargetPrice)
    {
        //public double TargetPrice { get; init; }

        public DateTime? Expiry { get; init; }

        public long? PositionId { get; init; }

        public int RiskUserId { get; init; }

        public string Designation { get; init; }
    }

    public static async Task<string> AuthenticateAsync(string url, string email, string password)
    {
        using var client = new HttpClient();

        var loginData = new
        {
            email,
            Password = password
        };

        var response = await client.PostAsJsonAsync(url, loginData);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
            var token = responseData?.Data;

            if (!string.IsNullOrEmpty(token))
            {
                //StoreToken(token);
                return token;
            }
        }

        return null;
    }

    public static async Task<dynamic> PostHttp(string url, string token, object item)
    {
        using var client = new HttpClient();

        string jsonBody = JsonConvert.SerializeObject(item);
        var httpContent = new StringContent(jsonBody, Encoding.UTF8, "application/json");


        var uriBuilder = new UriBuilder(url)
        {
            Query = $"token={token}"
        };

        HttpResponseMessage response = await client.PostAsync(uriBuilder.ToString(), httpContent);

        if(response.IsSuccessStatusCode) 
        {
            Console.WriteLine("Post request successful");
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return jsonResponse;
        } else
        {
            string failedResponse = $"Post request failed with status code {response.StatusCode}";
            Console.WriteLine(failedResponse);
            return failedResponse;
        }
    }

    public static async Task<dynamic> GetHttp(string url, string token)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response = await client.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Get request successful");
            string jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject(jsonResponse);
        }
        else
        {
            string failedResponse = $"Get request failed with status code {response.StatusCode}";
            Console.WriteLine(failedResponse);
            return failedResponse;
        }
    }

    public class LoginResponse
    {
        public string Data { get; set; }
    }
}