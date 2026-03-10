using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Claims;

[Authorize]
public class AdminController : Controller
{
    private readonly FirebaseService _firebase;

    public AdminController(FirebaseService firebase)
    {
        _firebase = firebase; 
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string usuario, string senha, string? returnUrl)
    {
        var admin = await _firebase.GetAdminPorUsuarioAsync(usuario);

        if (admin == null)
        {
            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }

        if (string.IsNullOrEmpty(admin.senhaHash) || !BCrypt.Net.BCrypt.Verify(senha, admin.senhaHash))
        {
            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.usuario),
            new Claim(ClaimTypes.NameIdentifier, admin.Id),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        if (!string.IsNullOrEmpty(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Admin");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }

    [Authorize]
    public async Task<IActionResult> Index(DateTime? data)
    {
        DateTime dataFiltro = data ?? DateTime.Today;

        var agendamentos = await _firebase.GetAgendamentosPorDataAsyncDash(dataFiltro);

        ViewBag.DataSelecionada = dataFiltro;

        ViewBag.Total = agendamentos.Count;
        ViewBag.Agendados = agendamentos.Count(a => a.Status == "Agendado");
        ViewBag.Cancelados = agendamentos.Count(a => a.Status == "Cancelado");
        ViewBag.Concluidos = agendamentos.Count(a => a.Status == "Concluido");

        return View(agendamentos);
    }

    [Authorize]
    public async Task<IActionResult> Cancelar(string id)
    {
        await _firebase.AtualizarStatusAsync(id, "Cancelado");
        return RedirectToAction("Index");
    }

    [Authorize]
    public async Task<IActionResult> Concluir(string id)
    {
        await _firebase.AtualizarStatusAsync(id, "Concluido");
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult AlterarSenha()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AlterarSenha(TrocarSenhaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(adminId))
            {
                return BadRequest("Admin não identificado.");
            }

            await _firebase.AlterarSenha(adminId, model.SenhaAtual, model.NovaSenha);

            TempData["ToastSucesso"] = "Senha alterada com sucesso!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ToastErro"] = ex.Message;
            return RedirectToAction("AlterarSenha");
        }
    }

}