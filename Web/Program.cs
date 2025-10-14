using Application.Interfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Web.Configurations;


var builder = WebApplication.CreateBuilder(args);

// Configure Telegram settings
builder.Services.Configure<TelegramSettings>(
    builder.Configuration.GetSection(TelegramSettings.SectionName));

// Register Telegram Bot Client
builder.Services.AddSingleton<TelegramBotClient>(provider =>
{
    var settings = provider.GetRequiredService<IOptions<TelegramSettings>>().Value;
    return new TelegramBotClient(settings.BotToken);
});
builder.Services.AddScoped<ITelegramService, TelegramService>();

// Register application services
builder.Services.AddControllersWithViews();

// Add logging
builder.Services.AddLogging();
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
    pattern: "{controller=Folder}/{action=Index}/{id?}")
   .WithStaticAssets();


using (var scope = app.Services.CreateScope())
{
    var telegramService = scope.ServiceProvider.GetRequiredService<ITelegramService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var isConnected = await telegramService.TestConnectionAsync();
        if (isConnected)
        {
            logger.LogInformation("? Telegram Bot connection successful");
        }
        else
        {
            logger.LogWarning("??  Telegram Bot connection failed");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "? Error testing Telegram connection");
    }
}

app.Run();
