using Application.Services;
using Domain.Interfaces;
using Infrastructure.MessageBus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IMessageBusConsumer, RabbitMqConsumer>();
builder.Services.AddHostedService<UserCreatedEventService>();
builder.Services.AddHostedService<PaymentProcessedEventService>();

var app = builder.Build();

app.Run();





