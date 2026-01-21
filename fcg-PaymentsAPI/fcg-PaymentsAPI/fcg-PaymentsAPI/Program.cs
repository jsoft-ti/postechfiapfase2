using Application.Services;
using Domain.Interfaces;
using Infrastructure.MessageBus;

var builder = WebApplication.CreateBuilder(args);

// Configure RabbitMqOptions
var rabbitMqOptions = new RabbitMqOptions();
builder.Configuration.GetSection("RabbitMq").Bind(rabbitMqOptions);
builder.Services.AddSingleton(rabbitMqOptions);

builder.Services.AddSingleton<IMessageBusConsumer, RabbitMqConsumer>();
builder.Services.AddSingleton<IMessageBusProducer, RabbitMqProducer>();

builder.Services.AddHealthChecks();

builder.Services.AddHostedService<OrderPlacedEventService>();


var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
