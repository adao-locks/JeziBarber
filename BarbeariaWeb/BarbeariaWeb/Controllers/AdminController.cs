using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    private readonly FirebaseService _firebase;

    public AdminController(FirebaseService firebase)
    {
        _firebase = firebase;
    }

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