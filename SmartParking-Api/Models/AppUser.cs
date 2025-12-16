using System;

namespace SmartParking_Api.Models;

public class AppUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = "";  // "user"
    public string Email { get; set; } = "";
    public string Role { get; set; } = "User";  // "User" | "Admin"

    public string PasswordHash { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}