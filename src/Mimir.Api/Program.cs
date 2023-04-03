using System.Net.Http.Headers;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Mimir.Api.Configurations;
using Mimir.Api.Security;
using Mimir.Application.ChatGpt;
using Mimir.Application.Conversations.CreateConversation;
using Refit;

var builder = WebApplication.CreateBuilder(args);

// logging
builder.Logging
    .ClearProviders()
    .AddConsole();

// health checks
builder.Services.AddHealthChecks();
// Add FastEndpoints framework services
builder.Services.AddFastEndpoints();

// Add Auth0 authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var identityProviderOptions = new IdentityProviderOptions();
        builder.Configuration.Bind("IdP", identityProviderOptions);
        options.Authority = identityProviderOptions.Authority;
        options.Audience = identityProviderOptions.Audience;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ChatGptUserOnly", x => x.RequireAuthenticatedUser()
        .RequireScope("write:chatgpt"));
});

builder.Services.AddSwaggerDoc(s =>
{
    s.DocumentName = "API 1.0";
    s.Title = "Mimir API";
    s.Version = "v1.0";
});

// AWS services
var awsOptions = builder.Configuration.GetAWSOptions();
if (awsOptions.DefaultClientConfig.ServiceURL?.StartsWith("http://localhost") == true)
{
    // it doesn't matter what credentials we use here, because we're using localstack.
    awsOptions.Credentials = new BasicAWSCredentials("test", "test");
}
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();

// 3rd party API
var chatGptOptions = new ChatGptOptions();
builder.Configuration.Bind("ChatGpt", chatGptOptions);
builder.Services
    .AddRefitClient<IChatGptApi>()
    .ConfigureHttpClient(c =>
    {
        if (string.IsNullOrWhiteSpace(chatGptOptions.ApiKey))
        {
            throw new InvalidOperationException("Missing ChatGpt:ApiKey. Please check your configuration.");
        }

        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", chatGptOptions.ApiKey);
        c.BaseAddress = new Uri(chatGptOptions.ApiDomain);
    });

// add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateConversationRequest>();
});

// add AWS Lambda support.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// kestrel configs
builder.WebHost.UseUrls("http://*:5000");

var app = builder.Build();
app.MapHealthChecks("/healthy");

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen(); 
}

await app.RunAsync();