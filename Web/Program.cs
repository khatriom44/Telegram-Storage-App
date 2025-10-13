using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Application.Interfaces;
using Infrastructure.Services;
using Infrastructure.Repositories;
using Application.Handlers;
using MediatR;
using Web.Configurations;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection("Telegram"));
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<TelegramSettings>>().Value;
    var httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(10) // adjust as needed
    };
    return new TelegramBotClient(config.BotToken);
});
// Register telegram service implementation for metadata handling
builder.Services.AddScoped<ITelegramService, TelegramService>();

builder.Services.AddScoped<ITelegramFileService, TelegramFileService>();
builder.Services.AddScoped<IFileRepository, TelegramFileRepository>();

builder.Services.AddScoped<IVideoChunkService, VideoChunkService>();

// Register MediatR with updated syntax for latest versions
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblyContaining<UploadFileCommandHandler>());

// Add controllers and views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

app.Run();
