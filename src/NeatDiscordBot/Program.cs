using NeatDiscordBot;
using NeatDiscordBot.Discord;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDiscordApi();
builder.Services.AddNeatServices();
builder.Services.AddHostedService<NeatClientWorker>();

var host = builder.Build();
host.Services.EnableFeatures();

await host.RunAsync();