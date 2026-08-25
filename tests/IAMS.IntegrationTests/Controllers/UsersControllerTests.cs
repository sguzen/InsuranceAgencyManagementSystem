using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IAMS.IntegrationTests.Fixtures;
using IAMS.Shared.DTOs.Identity;
using IAMS.Shared.Models;

namespace IAMS.IntegrationTests.API.Controllers;

/// <summary>
/// The Web's UsersApiClient deserializes every response as Result / Result&lt;T&gt; and calls the
/// routes exercised here, so these tests pin the contract between IAMS.Web and IAMS.Api.
/// </summary>
public class UsersControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersControllerTests(TestWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Tenant-ID", "test-agency-1");
    }

    private async Task<UserDto> CreateUserAsync(string email, string password = "Initial123")
    {
        var response = await _client.PostAsJsonAsync("/api/users", new CreateUserDto
        {
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var result = await response.Content.ReadFromJsonAsync<Result<UserDto>>();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        return result.Data!;
    }

    [Fact]
    public async Task GetAllUsers_ReturnsResultEnvelopeWithList()
    {
        await CreateUserAsync("list@test.local");

        var response = await _client.GetAsync("/api/users/all");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<List<UserDto>>>();
        result.Should().NotBeNull();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().Contain(u => u.Email == "list@test.local");
    }

    [Fact]
    public async Task GetUsers_Paged_ReturnsPagedResultEnvelope()
    {
        await CreateUserAsync("paged@test.local");

        var response = await _client.GetAsync("/api/users?page=1&pageSize=5&search=paged");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<PagedResult<UserDto>>>();
        result!.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().ContainSingle(u => u.Email == "paged@test.local");
        result.Data.TotalCount.Should().Be(1);
        result.Data.PageSize.Should().Be(5);
    }

    [Fact]
    public async Task GetAllRoles_ReturnsResultEnvelope()
    {
        var response = await _client.GetAsync("/api/users/roles");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<List<string>>>();
        result!.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUser_UnknownId_ReturnsNotFoundEnvelope()
    {
        var response = await _client.GetAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var result = await response.Content.ReadFromJsonAsync<Result<UserDto>>();
        result!.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateUser_WithUnknownRole_ReturnsValidationFailure()
    {
        var response = await _client.PostAsJsonAsync("/api/users", new CreateUserDto
        {
            Email = "badrole@test.local",
            Password = "Initial123",
            ConfirmPassword = "Initial123",
            FirstName = "Bad",
            LastName = "Role",
            Roles = new List<string> { "DoesNotExist" }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<Result<UserDto>>();
        result!.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("DoesNotExist"));
    }

    [Fact]
    public async Task ChangePassword_WithoutCurrentPassword_ActsAsAdminReset()
    {
        var user = await CreateUserAsync("reset@test.local");

        var response = await _client.PostAsJsonAsync($"/api/users/{user.Id}/change-password", new ChangePasswordDto
        {
            NewPassword = "Reset12345",
            ConfirmNewPassword = "Reset12345"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var result = await response.Content.ReadFromJsonAsync<Result>();
        result!.IsSuccess.Should().BeTrue();

        // The new password is now the only one accepted for a self-service change.
        var wrongOld = await _client.PostAsJsonAsync($"/api/users/{user.Id}/change-password", new ChangePasswordDto
        {
            CurrentPassword = "Initial123",
            NewPassword = "Another123",
            ConfirmNewPassword = "Another123"
        });
        wrongOld.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var wrongOldResult = await wrongOld.Content.ReadFromJsonAsync<Result>();
        wrongOldResult!.IsSuccess.Should().BeFalse();
        wrongOldResult.Errors.Should().NotBeEmpty();

        var rightOld = await _client.PostAsJsonAsync($"/api/users/{user.Id}/change-password", new ChangePasswordDto
        {
            CurrentPassword = "Reset12345",
            NewPassword = "Another123",
            ConfirmNewPassword = "Another123"
        });
        rightOld.StatusCode.Should().Be(HttpStatusCode.OK, await rightOld.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task ChangePassword_WeakPassword_ReturnsIdentityErrors()
    {
        var user = await CreateUserAsync("weak@test.local");

        var response = await _client.PostAsJsonAsync($"/api/users/{user.Id}/change-password", new ChangePasswordDto
        {
            NewPassword = "alllowercase1",
            ConfirmNewPassword = "alllowercase1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<Result>();
        result!.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ChangePassword_MismatchedConfirmation_ReturnsValidationFailure()
    {
        var user = await CreateUserAsync("mismatch@test.local");

        var response = await _client.PostAsJsonAsync($"/api/users/{user.Id}/change-password", new ChangePasswordDto
        {
            NewPassword = "Mismatch123",
            ConfirmNewPassword = "Different123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<Result>();
        result!.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("match"));
    }

    [Fact]
    public async Task SetStatus_And_Update_RoundTrip()
    {
        var user = await CreateUserAsync("status@test.local");

        var status = await _client.PutAsJsonAsync($"/api/users/{user.Id}/status", new { IsActive = false });
        status.StatusCode.Should().Be(HttpStatusCode.OK);
        (await status.Content.ReadFromJsonAsync<Result>())!.IsSuccess.Should().BeTrue();

        var update = await _client.PutAsJsonAsync($"/api/users/{user.Id}", new UpdateUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = "Renamed",
            LastName = "User",
            IsActive = true
        });
        update.StatusCode.Should().Be(HttpStatusCode.OK, await update.Content.ReadAsStringAsync());

        var fetched = await _client.GetFromJsonAsync<Result<UserDto>>($"/api/users/{user.Id}");
        fetched!.Data!.FirstName.Should().Be("Renamed");
        fetched.Data.IsActive.Should().BeTrue();
    }
}
