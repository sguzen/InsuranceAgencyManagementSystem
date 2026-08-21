using System.ComponentModel.DataAnnotations;

/// <summary>
/// Password change request for <c>POST api/users/{id}/change-password</c>.
/// <see cref="CurrentPassword"/> is optional: when present the change is verified against it
/// (self-service); when absent the call is an administrative reset. Confirmation matching and
/// password strength are enforced by the API, not by attributes, so both flows share one DTO.
/// </summary>
public class ChangePasswordDto
{
    public string? CurrentPassword { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; } = string.Empty;

    public string? ConfirmNewPassword { get; set; }
}
