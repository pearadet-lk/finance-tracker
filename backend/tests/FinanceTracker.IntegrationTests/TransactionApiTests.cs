using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FinanceTracker.IntegrationTests;

public class TransactionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TransactionApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                "Host=localhost;Database=finance_tracker_test;Username=admin;Password=password");
        }).CreateClient();
    }

    [Fact]
    public async Task Get_Transactions_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/transactions");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_Transaction_WithValidData_Returns201()
    {
        var token = await GetAuthToken();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            amount = 100.00,
            type = "Expense",
            categoryId = "a1000000-0000-0000-0000-000000000003",
            description = "Integration test transaction",
            date = DateTime.UtcNow
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var response = await _client.PostAsync("/api/transactions", content);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task<string> GetAuthToken()
    {
        var login = new { email = "test@test.com", password = "Test123!" };
        var content = new StringContent(JsonSerializer.Serialize(login), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString()!;
    }
}
