namespace ColdFishWMS.Models.DTOs
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }   
        public string Message { get; set; } = string.Empty;

        public string? Token { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? RoleName { get; set; }
    }
}
