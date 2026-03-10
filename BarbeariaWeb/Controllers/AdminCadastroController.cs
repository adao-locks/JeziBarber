using Microsoft.AspNetCore.Authorization; // Adicione esta linha
using Microsoft.AspNetCore.Mvc;

[Authorize] // Adicione esta linha no nível da classe
public class AdminCadastroController : Controller
{
    private readonly FirebaseService _firebase;

    public AdminCadastroController(FirebaseService firebase)
    {
        _firebase = firebase;
    }

    // ... (mantenha todos os métodos existentes)
    public async Task<IActionResult> Barbeiros()
    {
        var lista = await _firebase.GetTodosBarbeirosAsync();
        return View(lista);
    }

    [HttpPost]
    public async Task<IActionResult> CriarBarbeiro(string nome)
    {
        await _firebase.AddBarbeiroAsync(new Barbeiro
        {
            Nome = nome,
            Ativo = true
        });

        return RedirectToAction("Barbeiros");
    }

    public async Task<IActionResult> ToggleBarbeiro(string id, bool ativo)
    {
        await _firebase.AtualizarStatusBarbeiroAsync(id, ativo);
        return RedirectToAction("Barbeiros");
    }

    public async Task<IActionResult> Servicos()
    {
        var lista = await _firebase.GetTodosServicosAsync();
        return View(lista);
    }

    [HttpPost]
    public async Task<IActionResult> CriarServico(string descricao, int duracaoMinutos, double valor)
    {
        await _firebase.AddServicoAsync(new Servico
        {
            Descricao = descricao,
            DuracaoMinutos = duracaoMinutos,
            Valor = valor,
            Ativo = true
        });

        return RedirectToAction("Servicos");
    }

    public async Task<IActionResult> ToggleServico(string id, bool ativo)
    {
        await _firebase.AtualizarStatusServicoAsync(id, ativo);
        return RedirectToAction("Servicos");
    }
}