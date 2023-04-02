using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// logging
builder.Logging
    .ClearProviders()
    .AddJsonConsole();

// health checks
builder.Services.AddHealthChecks();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// add Swagger/OpenAPI support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// add AWS Lambda support.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// kestrel configs
builder.WebHost.UseUrls("http://*:5000");

var app = builder.Build();
app.MapHealthChecks("/healthy");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/document/{name}", (string name) =>
{
    
    return name;
});

await app.RunAsync();