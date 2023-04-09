using System.Diagnostics;
using System.Net.Http.Headers;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using FastEndpoints;
using FastEndpoints.Swagger;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Mimir.Api.Configurations;
using Mimir.Api.HttpMocks;
using Mimir.Api.Model.Mapping;
using Mimir.Api.Security;
using Mimir.Application.Features.CreateConversation;
using Mimir.Application.Interfaces;
using Mimir.Application.OpenAI;
using Mimir.Infrastructure.Configurations;
using Mimir.Infrastructure.Impl;
using Mimir.Infrastructure.OpenAI;
using Mimir.Infrastructure.Repositories;
using Refit;
using IMapper = AutoMapper.IMapper;

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

// register Options
builder.Services.AddOptions<DynamoDbOptions>().Bind(builder.Configuration.GetSection(DynamoDbOptions.Key))
    .ValidateDataAnnotations()
    .ValidateOnStart();

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

// repositories
builder.Services.Scan(s => s
    .FromAssembliesOf(typeof(ConversationRepository))
    .AddClasses(c => c.InNamespaceOf(typeof(ConversationRepository)))
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// application interfaces
builder.Services.AddSingleton<IDateTime, DateTimeMachine>();

// GPT API registration
var chatGptOptions = new OpenAIOptions();
builder.Configuration.Bind("ChatGpt", chatGptOptions);
builder.Services.Configure<OpenAIOptions>(
    builder.Configuration.GetSection(OpenAIOptions.Key));
builder.Services.AddOptions<OpenAIOptions>()
    .Bind(builder.Configuration.GetSection(OpenAIOptions.Key))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IChatGptApi, ChatGptApi>();
// var httpClientBuilder = builder.Services
//     .AddRefitClient<IChatGptApi>()
//     .ConfigureHttpClient(c =>
//     {
//         if (string.IsNullOrWhiteSpace(chatGptOptions.ApiKey))
//         {
//             throw new InvalidOperationException("Missing ChatGpt:ApiKey. Please check your configuration.");
//         }
//
//         c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", chatGptOptions.ApiKey);
//         c.BaseAddress = new Uri(chatGptOptions.ApiDomain);
//     });
// if (builder.Configuration.GetValue<bool>("ChatGpt:UseMock"))
// {
//     builder.Services.AddSingleton<ChatGptApiHandlerMock>();
//     httpClientBuilder.ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<ChatGptApiHandlerMock>());
// }

// add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<CreateConversationCommandHandler>();
});

// automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// add AWS Lambda support.
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// CORS
builder.Services.AddCors(cors =>
{
    cors.AddPolicy("AllowLocal", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// kestrel configs
builder.WebHost.UseUrls("http://*:5000");

var app = builder.Build();
app.MapHealthChecks("/healthy");

app.UseCors("AllowLocal");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen(); 
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/plain";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        Debug.Assert(exceptionHandlerPathFeature != null);
        var exception = exceptionHandlerPathFeature.Error;

        // Log the exception
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "An unhandled exception has occurred");
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});

using var scope = app.Services.CreateScope();
var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
mapper.ConfigurationProvider.AssertConfigurationIsValid();

await app.RunAsync();

// for integration test
[UsedImplicitly]
public partial class Program
{
}