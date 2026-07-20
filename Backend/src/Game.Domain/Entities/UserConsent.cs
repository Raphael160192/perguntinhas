namespace Game.Domain.Entities;

// Um aceite de consentimento LGPD registrado (D3). Append-only.
public class UserConsent
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ConsentType { get; set; } = string.Empty;
    public string PolicyVersion { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
