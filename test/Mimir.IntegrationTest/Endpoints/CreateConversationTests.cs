using System.Net;
using System.Net.Http.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Api.Model.Conversations;
using Mimir.Application.Features.CreateConversation;
using Mimir.Application.OpenAI;
using Mimir.Domain.Models;
using Mimir.Infrastructure.Configurations;

namespace Mimir.IntegrationTest.Endpoints;

public class CreateConversationTests : EndpointTestBase
{
    public CreateConversationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Create_a_conversation()
    {
        var fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
        var client = Factory
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<IChatGptApi>(_ => fixture.Create<IChatGptApi>());
                });
            })
            .CreateClient();
        var dynamoDb = Factory.Services.GetRequiredService<IAmazonDynamoDB>();
        var tableName = Factory.Services.GetRequiredService<IConfiguration>()
            .GetSection($"{DynamoDbOptions.Key}:TableName").Value;

        var response = await client.PostAsJsonAsync("/v1/conversations", new CreateConversationRequest
        {
            Message = "Hello"
        });

        response.EnsureSuccessStatusCode();
        var actualResponse = await response.Content.ReadFromJsonAsync<CreateConversationResponse>();
        actualResponse.Should().NotBeNull();
        var conversationMessages = await dynamoDb.QueryAsync(new QueryRequest
        {
            KeyConditionExpression = "PK = :pk and begins_with(SK, :sk)",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue($"CONVERSATION#{actualResponse!.Id}") },
                { ":sk", new AttributeValue("MESSAGE#") }
            },
            TableName = tableName
        });
        conversationMessages.Items.Should().HaveCount(1);
        conversationMessages.Items.First()["Role"].S.Should().Be("system");
        conversationMessages.Items.First()["Content"].S.Should().Be(SystemPrompt.DefaultPrompt);
    }

    [Fact]
    public async Task Return_bad_request_given_invalid_request()
    {
        var client = Factory
            .CreateClient();

        var response = await client.PostAsJsonAsync("/v1/conversations", new CreateConversationRequest
        {
            Message = string.Empty
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}