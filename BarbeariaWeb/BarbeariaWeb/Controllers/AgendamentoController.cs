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
    public async Task<IActionResult> HorariosDisponiveis(
    string barbeiroId,
    string servicoId,
    DateTime data)
    {
        var horarios = await GetHorariosDisponiveisInterno(barbeiroId, servicoId, data);
        return Json(horarios);
    }

    private IEnumerable<DateTime> GerarSlots(DateTime inicio, DateTime fim, int duracaoMinutosServico)
    {
        if (duracaoMinutosServico <= 0)
        {
            throw new ArgumentException($"A duração do serviço deve ser maior que zero. Valor recebido: {duracaoMinutosServico}", nameof(duracaoMinutosServico));
        }

        for (var dt = inicio; dt < fim; dt = dt.AddMinutes(60))
            yield return dt;
    }

    [HttpGet]
    public async Task<IActionResult> DiasDisponiveis(string barbeiroId, string servicoId, int ano, int mes)
    {
        var resultado = new List<object>();

        int diasNoMes = DateTime.DaysInMonth(ano, mes);

        for (int dia = 1; dia <= diasNoMes; dia++)
        {
            DateTime data = new DateTime(ano, mes, dia);

            if (data.Date < DateTime.Today)
            {
                resultado.Add(new
                {
                    data = data.ToString("yyyy-MM-dd"),
                    temDisponibilidade = false
                });
                continue;
            }

            var horarios = await GetHorariosDisponiveisInterno(barbeiroId, servicoId, data);

            resultado.Add(new
            {
                data = data.ToString("yyyy-MM-dd"),
                temDisponibilidade = horarios.Any()
            });
        }

        return Json(resultado);
    }

    private async Task<List<string>> GetHorariosDisponiveisInterno(
    string barbeiroId,
    string servicoId,
    DateTime data)
    {
        var servicos = await _firebase.GetServicosAsync();
        var servico = servicos.FirstOrDefault(s => s.Id == servicoId);

        if (servico == null)
            return new List<string>();

        var agendamentos = await _firebase.GetAgendamentosPorDataAsync(barbeiroId, data);

        List<string> horariosLivres = new();

        DateTime inicioExpediente = data.Date.AddHours(9);
        DateTime fimExpediente = data.Date.AddHours(19);

        for (DateTime horario = inicioExpediente;
             horario.AddMinutes(servico.DuracaoMinutos) <= fimExpediente;
             horario = horario.AddMinutes(30))
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

        return horariosLivres;
    }
}