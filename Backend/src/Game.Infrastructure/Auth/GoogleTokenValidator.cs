using Game.Application.Auth;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Game.Infrastructure.Auth;

// Valida o ID token do Google (assinatura, expiração e audience) usando as chaves
// públicas do Google via Google.Apis.Auth. A audience precisa bater com o nosso
// OAuth Client ID — token de outro app é rejeitado.
public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options;
    private readonly ILogger<GoogleTokenValidator> _logger;

    public GoogleTokenValidator(IOptions<GoogleAuthOptions> options, ILogger<GoogleTokenValidator> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken) || string.IsNullOrWhiteSpace(_options.ClientId))
        {
            return null;
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return new GoogleUserInfo(payload.Subject, payload.Email);
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning("ID token do Google rejeitado: {Reason}", ex.Message);
            return null;
        }
    }
}
