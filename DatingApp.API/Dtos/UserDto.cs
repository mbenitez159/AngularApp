using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(12,MinimumLength = 5,ErrorMessage = "You most specify password between 5 and 12 characters")]
        public string Password { get; set; }
    }
}