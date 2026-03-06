using System.ComponentModel.DataAnnotations;

public class TrocarSenhaViewModel
{
    [Required(ErrorMessage = "Informe a senha atual")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha atual")]
    public string SenhaAtual { get; set; }

    [Required(ErrorMessage = "Informe a nova senha")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "A senha deve ter pelo menos 6 caracteres")]
    [Display(Name = "Nova senha")]
    public string NovaSenha { get; set; }

    [Required(ErrorMessage = "Confirme a nova senha")]
    [DataType(DataType.Password)]
    [Compare("NovaSenha", ErrorMessage = "As senhas não coincidem")]
    [Display(Name = "Confirmar senha")]
    public string ConfirmarSenha { get; set; }
}