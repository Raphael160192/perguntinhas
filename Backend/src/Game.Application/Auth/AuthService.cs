using Game.Application.Repositories;
using Game.Domain.Entities;

namespace Game.Application.Auth;

// Fluxo de login com Google (US6): valida o ID token uma única vez e emite o
// JWT próprio do backend (D6) com o UserId interno. Login é opcional (D1) —
// nenhum endpoint de jogo depende deste serviço.
public class AuthService : IAuthService
{
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUserRepository _userRepository;
    private readonly IJwtIssuer _jwtIssuer;

    public AuthService(
        IGoogleTokenValidator googleTokenValidator,
        IUserRepository userRepository,
        IJwtIssuer jwtIssuer)
    {
        _googleTokenValidator = googleTokenValidator;
        _userRepository = userRepository;
        _jwtIssuer = jwtIssuer;
    }

    public async Task<LoginResultDto?> LoginWithGoogleAsync(string idToken)
    {
        var googleUser = await _googleTokenValidator.ValidateAsync(idToken);
        if (googleUser is null)
        {
            return null;
        }

        var user = await _userRepository.FindByGoogleSubjectAsync(googleUser.Subject);

        if (user is null)
        {
            // Conta criada antes pelo fluxo de e-mail (US7): vincula o Google a ela
            // em vez de criar duplicata (Email tem índice único).
            user = await _userRepository.FindByEmailAsync(googleUser.Email);
        }

        if (user is null)
        {
            user = await _userRepository.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                GoogleSubject = googleUser.Subject,
                Email = googleUser.Email,
                AuthProvider = "google",
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            });
        }
        else
        {
            user.GoogleSubject = googleUser.Subject;
            user.AuthProvider = "google";
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        return new LoginResultDto
        {
            Token = _jwtIssuer.Issue(user.Id),
            UserId = user.Id,
            Email = user.Email
        };
    }
}
