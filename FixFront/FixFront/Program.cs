﻿using FixFront.Models;
using Microsoft.AspNetCore.SignalR.Client;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

class Program
{
    private static HubConnection connection;

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
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
            };

            var connectionRabbit = factory.CreateConnection();

            using var channel = connectionRabbit.CreateModel();

            channel.ConfirmSelect();

            channel.QueueDeclare("booking-test04", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var jsonString = JsonSerializer.Serialize(quote);

            var body = Encoding.UTF8.GetBytes(jsonString);

            channel.BasicPublish(exchange: "", routingKey: "booking-test04", basicProperties: null, body: body);
        }
    }
}