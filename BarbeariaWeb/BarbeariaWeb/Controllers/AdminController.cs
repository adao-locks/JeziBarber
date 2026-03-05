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
        Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("123"));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Login(string usuario, string senha)
    {
        var admin = await _firebase.GetAdminPorUsuarioAsync(usuario);

        if (admin == null)
        {
            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }

        bool senhaValida = BCrypt.Net.BCrypt.Verify(senha, admin.senhaHash);

        if (!senhaValida)
        {
            ViewBag.Erro = "Usuário ou senha inválidos.";
            return View();
        }

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.usuario),
                new Claim(ClaimTypes.Role, "Admin")
            };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

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

    public async Task<IActionResult> Cancelar(string id)
    {
        await _firebase.AtualizarStatusAsync(id, "Cancelado");
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Concluir(string id)
    {
        await _firebase.AtualizarStatusAsync(id, "Concluido");
        return RedirectToAction("Index");
    }
}