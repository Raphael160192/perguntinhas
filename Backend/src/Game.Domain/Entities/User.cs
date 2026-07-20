namespace Game.Domain.Entities;

// Conta de usuário (identidade persistente). Sistema separado do acesso via
// cartão/token físico — ver épico 2.
public class User
{
    public Guid Id { get; set; }
    public string? GoogleSubject { get; set; }
    public string Email { get; set; } = string.Empty;
    public string AuthProvider { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
