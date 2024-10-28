using FixFront.Models;
using FixFront.Services;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

class Program
{
    private static HubConnection connection;
    private static InfluxDBService _service = new InfluxDBService();

    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7261/trade")
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

        connection.On("Test", (object data) =>
        {
            Console.WriteLine($"Success => {data}");
        });

        await connection.StartAsync();
        Console.WriteLine("Connected to the Trade Hub");

        await connection.InvokeAsync("Ping");
        await connection.InvokeAsync("ConnectCentroid", "AliTest");

        //await connection.InvokeAsync("Test", "AliTest");

        var cts = new CancellationTokenSource();

        await FetchQuotes(connection, "AliTest", cts.Token);

        Console.WriteLine("Enter trade info to send (or 'exit' to quit):");
        Console.ReadLine();

        await connection.StopAsync();
        await connection.DisposeAsync();
    }

    static async Task FetchQuotes(HubConnection connection, string token, CancellationToken cancellationToken)
    {
        var quotes = connection.StreamAsync<SymbolQuote>("SymbolQuotes", token, cancellationToken);

        Console.WriteLine("Fetching Quotes!");

        await foreach (var quote in quotes)
        {

            Console.WriteLine($"Quote Symbol:{quote.SymbolName} Bid:{quote.Bid} Ask:{quote.Ask}");

            _service.Write(write =>
            {
                DateTime original = DateTime.Now; // Current date and time

                // Round down to the nearest 10 minutes
                int minutes = original.Minute / 10 * 10;
                DateTime truncated = new DateTime(
                    original.Year,
                    original.Month,
                    original.Day,
                    original.Hour,
                    original.Minute,   // Use the rounded minute value
                    0          // Set seconds to 0
                );

                //var point = PointData.Measurement($"Symbols-{truncated}")
                var point = PointData.Measurement($"Symbols-{truncated}")
                    .Tag("Id", quote.SymbolId.ToString())
                    .Field("symbolObject", JsonSerializer.Serialize(quote))
                    //.Field("BId", symbol.Bid)
                    //.Field("ASK", symbol.Ask)
                    //.Field("Digits", symbol.Digits)
                    .Timestamp(DateTime.Now, WritePrecision.Ms);

                write.WritePoint(point, "Symbols", "organization");
                //    channel.BasicAck(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                //    Console.WriteLine($"Symbol has been added{symbol.SymbolId}");
                //    var factory = new ConnectionFactory
                //{
                //    HostName = "localhost",
                //};

                //var connectionRabbit = factory.CreateConnection();

                //using var channel = connectionRabbit.CreateModel();

                //channel.ConfirmSelect();

                //channel.QueueDeclare("booking-test04", durable: true, exclusive: false, autoDelete: false, arguments: null);

                //var jsonString = JsonSerializer.Serialize(quote);

                //var body = Encoding.UTF8.GetBytes(jsonString);

                //channel.BasicPublish(exchange: "", routingKey: "booking-test04", basicProperties: null, body: body);
            });
        }
    }
}