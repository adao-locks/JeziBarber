using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;

public class AgendamentoController : Controller
{
    private readonly FirebaseService _firebase;

    public AgendamentoController(FirebaseService firebase)
    {
        _firebase = firebase;
    }

    public async Task<IActionResult> Index()
    {
        var barbeiros = await _firebase.GetBarbeirosAsync();
        var servicos = await _firebase.GetServicosAsync();

        ViewBag.Barbeiros = barbeiros;
        ViewBag.Servicos = servicos;
        ViewBag.Horarios = GerarHorariosPadrao();

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Criar(string nome, string telefone, string barbeiroId, string servicoId, DateTime dataHora)
    {
        if (dataHora < DateTime.Now)
        {
            TempData["Erro"] = "Não é possível agendar para datas passadas.";
            return RedirectToAction("Index");
        }

        bool ocupado = await _firebase.HorarioOcupadoAsync(barbeiroId, dataHora);

        if (ocupado)
        {
            TempData["Erro"] = "Este horário já foi reservado. Escolha outro.";
            return RedirectToAction("Index");
        }

        var agendamento = new Agendamento
        {
            NomeCliente = nome,
            Telefone = telefone,
            BarbeiroId = barbeiroId,
            ServicoId = servicoId,
            DataHora = Timestamp.FromDateTime(dataHora.ToUniversalTime()),
            Status = "Agendado"
        };

        await _firebase.AddAgendamentoAsync(agendamento);

        return RedirectToAction("Sucesso");
    }

    public IActionResult Sucesso()
    {
        return Content("Agendamento realizado com sucesso!");
    }

    [HttpGet]
    public async Task<IActionResult> HorariosDisponiveis(string barbeiroId, string servicoId, DateTime data)
    {
        var servicos = await _firebase.GetServicosAsync();
        var servico = servicos.FirstOrDefault(s => s.Id == servicoId);

        if (servico == null)
            return Json(new List<string>());

        var agendamentos = await _firebase.GetAgendamentosPorDataAsync(barbeiroId, data);

        List<string> horariosLivres = new();

        DateTime inicioExpediente = data.Date.AddHours(9);
        DateTime fimExpediente = data.Date.AddHours(19);

        for (DateTime horario = inicioExpediente;
             horario.AddMinutes(servico.DuracaoMinutos) <= fimExpediente;
             horario = horario.AddMinutes(30)) // intervalo de 30 min
        {
            DateTime novoInicio = horario;
            DateTime novoFim = horario.AddMinutes(servico.DuracaoMinutos);

            bool conflito = agendamentos.Any(a =>
            {
                DateTime existenteInicio = a.DataHora.ToDateTime().ToLocalTime();
                DateTime existenteFim = existenteInicio.AddMinutes(
                    servicos.First(s => s.Id == a.ServicoId).DuracaoMinutos);

                return novoInicio < existenteFim && novoFim > existenteInicio;
            });

            if (!conflito)
                horariosLivres.Add(novoInicio.ToString("HH:mm"));
        }

        return Json(horariosLivres);
    }

    private List<string> GerarHorariosPadrao()
    {
        var horarios = new List<string>();

        var inicio = new TimeSpan(8, 0, 0);
        var fim = new TimeSpan(18, 0, 0);
        var intervalo = TimeSpan.FromMinutes(30);

        for (var hora = inicio; hora < fim; hora += intervalo)
        {
            horarios.Add(hora.ToString(@"hh\:mm"));
        }

        return horarios;
    }
}