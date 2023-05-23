using Dolittle.SDK.Extensions.AspNet;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(
    "./data/appsettings.json",
    optional: false,
    reloadOnChange: true);
builder.Services.Configure<RaffleOptions>(
    builder.Configuration.GetSection(RaffleOptions.SectionName)
);
builder.Host.UseDolittle();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseDolittle();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Aigonix NDC 2023 Raffle");
    c.EnableTryItOutByDefault();
});

app.UseRouting();
var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webmanifest"] = "application/manifest+json";
provider.Mappings[".png"] = "image/png";
provider.Mappings[".ico"] = "image/x-icon";
provider.Mappings[".svg"] = "image/svg+xml";
app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "assets", "img")),
        RequestPath = "/assets/img",
        ContentTypeProvider = provider
    }
);
app.UseDefaultFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();