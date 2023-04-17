using System.Diagnostics;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using FastEndpoints;
using FastEndpoints.Swagger;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Mimir.Api.Configurations;
using Mimir.Api.Model.Mapping;
using Mimir.Api.Security;
using Mimir.Application.Features.CreateConversation;
using Mimir.Application.Interfaces;
using Mimir.Application.OpenAI;
using Mimir.Application.RealTime;
using Mimir.Application.Security;
using Mimir.Domain.Exceptions;
using Mimir.Infrastructure.Configurations;
using Mimir.Infrastructure.Impl;
using Mimir.Infrastructure.OpenAI;
using Mimir.Infrastructure.Repositories;
using OpenAI.GPT3.Extensions;
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

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/conversation"))
                    // Read the token out of the query string
                    context.Token = accessToken;
                return Task.CompletedTask;
            }
        };
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
if (awsOptions.DefaultClientConfig.ServiceURL?.StartsWith("http://localhost") == true ||
    awsOptions.DefaultClientConfig.ServiceURL?.StartsWith("http://host.docker.internal") == true)
    // it doesn't matter what credentials we use here,
    // because if we're using local DynamoDB, the credentials are ignored
    // but we need to set them to something, otherwise the AWS SDK will throw an exception
    awsOptions.Credentials = new BasicAWSCredentials("test", "test");
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
builder.Services.Configure<OpenAIOptions>(
    builder.Configuration.GetSection(OpenAIOptions.Key));
builder.Services.AddOptions<OpenAIOptions>()
    .Bind(builder.Configuration.GetSection(OpenAIOptions.Key))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOpenAIService(settings =>
{
    settings.ApiKey = builder.Configuration[$"{OpenAIOptions.Key}:ApiKey"];
});
builder.Services.AddScoped<IChatGptApi, ChatGptApi>();

// add MediatR
builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssemblyContaining<CreateConversationCommandHandler>(); });

// automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// add AWS Lambda support.
// builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// CORS
builder.Services.AddCors(cors =>
{
    cors.AddPolicy("AllowLocal", policyBuilder =>
    {
        policyBuilder.WithOrigins("http://localhost:3000", "https://www.askmimir.net")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// SignalR registration
builder.Services.AddSignalR();

// HTTP Context registration
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserIdentityProvider, HttpUserIdentityProvider>();

// kestrel configs
builder.WebHost.UseUrls("http://*:5000");

var app = builder.Build();
app.MapHealthChecks("/healthz");

app.UseCors("AllowLocal");
app.MapHub<ConversationHub>("/hubs/conversation");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.UseSwaggerGen();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/plain";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        Debug.Assert(exceptionHandlerPathFeature != null);
        var exception = exceptionHandlerPathFeature.Error;
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        switch (exception)
        {
            case OpenAIAPIException or NoChoiceProvidedException:
                context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                await context.Response.WriteAsync("OpenAPI encountered an error. Please try again later.");
                logger.LogError(exception, "An unhandled exception has occurred");
                break;
            case InsufficientPermissionException:
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("You don't have permission to perform this action.");
                break;
            default:
                logger.LogError(exception, "An unhandled exception has occurred");
                await context.Response.WriteAsync("An unexpected error occurred.");
                break;
        }
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