using AspireAppLakeFS.Client.Client;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddOpenApi();

// Add services to the container.
builder.AddLakeFSClient("lakefs");

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/api/user", async (LakeFSClientFactory factory) =>
{
    var client = await factory.GetHttpClientAsync();
    var httpRepsonse = await client.GetAsync("/api/v1/user");
    var response = await httpRepsonse.Content.ReadFromJsonAsync<object>();
    return Results.Ok(response);
});

app.MapGet("/api/config", async (LakeFSClientFactory factory) =>
{
    var client = await factory.GetHttpClientAsync();
    var httpRepsonse = await client.GetAsync("/api/v1/config");
    var response = await httpRepsonse.Content.ReadFromJsonAsync<object>();
    return Results.Ok(response);
});

app.MapPost("/api/setup_lakefs", async (LakeFSClientFactory factory) =>
{
    using StringContent jsonContent = new(
        JsonSerializer.Serialize(new
        {
            username = "admin",
            key = new
            {
                access_key_id = "AKIAIOSFODNN7EXAMPLE",
                secret_access_key = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY"
            }
        }),
        Encoding.UTF8,
        "application/json");

    var client = await factory.GetHttpClientAsync();
    var httpRepsonse = await client.PostAsync("/api/v1/setup_lakefs", jsonContent);
    var response = await httpRepsonse.Content.ReadFromJsonAsync<object>();
    return Results.Ok(response);
});

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();

app.Run();