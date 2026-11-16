using System.ComponentModel.DataAnnotations;

namespace InstitutoBlazor.Models;

public class RegistroUsuario
{
    [Required(ErrorMessage = "Ingrese un nombre de usuario.")]
    [RegularExpression(@"^[a-zA-Z0-9_]{3,20}$", ErrorMessage = "Use entre 3 y 20 letras, números o guiones bajos.")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Ingrese un correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingrese un email válido.")]
    [StringLength(200, ErrorMessage = "El correo no debe superar los 200 caracteres.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingrese una contraseña.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 128 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[0-9]).+$", ErrorMessage = "Incluya al menos una mayúscula y un número.")]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Confirme la contraseña.")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmarPassword { get; set; } = "";
}
