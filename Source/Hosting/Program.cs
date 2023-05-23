using Dolittle.SDK.Extensions.AspNet;

var builder = WebApplication.CreateBuilder(args);

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

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();