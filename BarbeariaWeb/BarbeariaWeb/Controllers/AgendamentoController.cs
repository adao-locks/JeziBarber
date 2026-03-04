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
        if (string.IsNullOrEmpty(barbeiroId) || string.IsNullOrEmpty(servicoId))
            return Json(new List<string>());

        var servicos = await _firebase.GetServicosAsync();
        var servico = servicos.FirstOrDefault(s => s.Id == servicoId);

        if (servico == null)
            return Json(new List<string>());

        var agendamentos = await _firebase.GetAgendamentosPorDataAsync(barbeiroId, data);

        var horariosLivres = new List<string>();

        var inicioExpediente = data.Date.AddHours(9);
        var fimExpediente = data.Date.AddHours(19);
        var agora = DateTime.Now;

        foreach (var slot in GerarSlots(inicioExpediente, fimExpediente, servico.DuracaoMinutos))
        {
            if (data.Date == agora.Date && slot <= agora)
                continue;

            var novoInicio = slot;
            var novoFim = slot.AddMinutes(servico.DuracaoMinutos);

            bool conflito = agendamentos.Any(a =>
            {
                var existenteInicio = a.DataHora.ToDateTime().ToLocalTime();
                var servicoExistente = servicos.FirstOrDefault(s => s.Id == a.ServicoId);
                if (servicoExistente == null) return false;

                var existenteFim = existenteInicio.AddMinutes(servicoExistente.DuracaoMinutos);

                return novoInicio < existenteFim && novoFim > existenteInicio;
            });

            if (!conflito && novoFim <= fimExpediente)
                horariosLivres.Add(novoInicio.ToString("HH:mm"));
        }

        return Json(horariosLivres);
    }

    private IEnumerable<DateTime> GerarSlots(DateTime inicio, DateTime fim, int duracaoMinutosServico)
    {
        if (duracaoMinutosServico <= 0)
        {
            throw new ArgumentException($"A duração do serviço deve ser maior que zero. Valor recebido: {duracaoMinutosServico}", nameof(duracaoMinutosServico));
        }

        for (var dt = inicio; dt < fim; dt = dt.AddMinutes(duracaoMinutosServico))
            yield return dt;
    }
}