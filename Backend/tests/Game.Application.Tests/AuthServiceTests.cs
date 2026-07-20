using Game.Application.Auth;
using Game.Application.Repositories;
using Game.Domain.Entities;
using Xunit;

namespace Game.Application.Tests;

// Fluxo de login com Google (US6): validação do ID token, criação/vínculo de
// conta e emissão do JWT próprio com o UserId interno.
public class AuthServiceTests
{
    [Fact]
    public async Task LoginWithGoogle_InvalidToken_ReturnsNull()
    {
        var repository = new FakeUserRepository();
        var service = new AuthService(new FakeGoogleValidator(null), repository, new FakeJwtIssuer());

        var result = await service.LoginWithGoogleAsync("token-invalido");

        Assert.Null(result);
        Assert.Empty(repository.Users);
    }

    [Fact]
    public async Task LoginWithGoogle_NewUser_CreatesAccountAndIssuesToken()
    {
        var repository = new FakeUserRepository();
        var issuer = new FakeJwtIssuer();
        var validator = new FakeGoogleValidator(new GoogleUserInfo("google-sub-1", "novo@exemplo.com"));
        var service = new AuthService(validator, repository, issuer);

        var result = await service.LoginWithGoogleAsync("token-valido");

        Assert.NotNull(result);
        var created = Assert.Single(repository.Users);
        Assert.Equal("google-sub-1", created.GoogleSubject);
        Assert.Equal("novo@exemplo.com", created.Email);
        Assert.Equal("google", created.AuthProvider);
        Assert.NotNull(created.LastLoginAt);
        Assert.Equal(created.Id, result.UserId);
        Assert.Equal($"jwt:{created.Id}", result.Token);
    }

    [Fact]
    public async Task LoginWithGoogle_ExistingUserBySubject_DoesNotDuplicateAndUpdatesLastLogin()
    {
        var existing = new User
        {
            Id = Guid.NewGuid(),
            GoogleSubject = "google-sub-1",
            Email = "veterano@exemplo.com",
            AuthProvider = "google",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            LastLoginAt = DateTime.UtcNow.AddDays(-10)
        };
        var repository = new FakeUserRepository(existing);
        var validator = new FakeGoogleValidator(new GoogleUserInfo("google-sub-1", "veterano@exemplo.com"));
        var service = new AuthService(validator, repository, new FakeJwtIssuer());

        var result = await service.LoginWithGoogleAsync("token-valido");

        Assert.NotNull(result);
        Assert.Single(repository.Users);
        Assert.Equal(existing.Id, result.UserId);
        Assert.True(repository.Users[0].LastLoginAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task LoginWithGoogle_ExistingUserByEmailOnly_LinksGoogleSubject()
    {
        // Conta criada antes pelo fluxo de e-mail (US7): sem GoogleSubject.
        var existing = new User
        {
            Id = Guid.NewGuid(),
            GoogleSubject = null,
            Email = "misto@exemplo.com",
            AuthProvider = "email",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        var repository = new FakeUserRepository(existing);
        var validator = new FakeGoogleValidator(new GoogleUserInfo("google-sub-9", "misto@exemplo.com"));
        var service = new AuthService(validator, repository, new FakeJwtIssuer());

        var result = await service.LoginWithGoogleAsync("token-valido");

        Assert.NotNull(result);
        var user = Assert.Single(repository.Users);
        Assert.Equal(existing.Id, result.UserId);
        Assert.Equal("google-sub-9", user.GoogleSubject);
        Assert.Equal("google", user.AuthProvider);
    }

    private sealed class FakeGoogleValidator(GoogleUserInfo? result) : IGoogleTokenValidator
    {
        public Task<GoogleUserInfo?> ValidateAsync(string idToken) => Task.FromResult(result);
    }

    private sealed class FakeJwtIssuer : IJwtIssuer
    {
        public string Issue(Guid userId) => $"jwt:{userId}";
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = new();

        public FakeUserRepository(params User[] seed) => Users.AddRange(seed);

        public Task<User?> FindByIdAsync(Guid id) =>
            Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

        public Task<User?> FindByEmailAsync(string email) =>
            Task.FromResult(Users.FirstOrDefault(u => u.Email == email));

        public Task<User?> FindByGoogleSubjectAsync(string googleSubject) =>
            Task.FromResult(Users.FirstOrDefault(u => u.GoogleSubject == googleSubject));

        public Task<User> AddAsync(User user)
        {
            Users.Add(user);
            return Task.FromResult(user);
        }

        public Task UpdateAsync(User user)
        {
            var index = Users.FindIndex(u => u.Id == user.Id);
            if (index >= 0) Users[index] = user;
            return Task.CompletedTask;
        }
    }
}
