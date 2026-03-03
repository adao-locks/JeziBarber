using Google.Cloud.Firestore;

public class FirebaseService
{
    private readonly FirestoreDb _db;

    public FirebaseService(FirestoreDb db)
    {
        _db = db;
    }

    public async Task<List<Barbeiro>> GetBarbeirosAsync()
    {
        var snapshot = await _db.Collection("barbeiros")
                                .WhereEqualTo("Ativo", true)
                                .GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ConvertTo<Barbeiro>()).ToList();
    }

    public async Task<List<Servico>> GetServicosAsync()
    {
        var snapshot = await _db.Collection("servicos")
                                .WhereEqualTo("Ativo", true)
                                .GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ConvertTo<Servico>()).ToList();
    }

    public async Task AddAgendamentoAsync(Agendamento agendamento)
    {
        await _db.Collection("agendamentos").AddAsync(agendamento);
    }
    public async Task<bool> HorarioOcupadoAsync(string barbeiroId, DateTime dataHora)
    {
        var inicio = dataHora;
        var fim = dataHora.AddMinutes(1);

        var snapshot = await _db.Collection("agendamentos")
            .WhereEqualTo("BarbeiroId", barbeiroId)
            .WhereEqualTo("Status", "Agendado")
            .WhereGreaterThanOrEqualTo("DataHora", Timestamp.FromDateTime(inicio.ToUniversalTime()))
            .WhereLessThan("DataHora", Timestamp.FromDateTime(fim.ToUniversalTime()))
            .GetSnapshotAsync();

        return snapshot.Count > 0;
    }

    public async Task<List<Agendamento>> GetAgendamentosAsync()
    {
        var snapshot = await _db.Collection("agendamentos")
                                .OrderByDescending("DataHora")
                                .GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ConvertTo<Agendamento>()).ToList();
    }

    public async Task AtualizarStatusAsync(string id, string novoStatus)
    {
        var docRef = _db.Collection("agendamentos").Document(id);
        await docRef.UpdateAsync("Status", novoStatus);
    }

    public async Task<List<Agendamento>> GetAgendamentosPorDataAsync(string barbeiroId, DateTime data)
    {
        var inicioDia = data.Date;
        var fimDia = inicioDia.AddDays(1);

        var snapshot = await _db.Collection("agendamentos")
            .WhereEqualTo("BarbeiroId", barbeiroId)
            .WhereEqualTo("Status", "Agendado")
            .WhereGreaterThanOrEqualTo("DataHora", Timestamp.FromDateTime(inicioDia.ToUniversalTime()))
            .WhereLessThan("DataHora", Timestamp.FromDateTime(fimDia.ToUniversalTime()))
            .GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ConvertTo<Agendamento>()).ToList();
    }

    public async Task<List<Agendamento>> GetAgendamentosPorDataAsyncDash(DateTime data)
    {
        var inicioDia = data.Date;
        var fimDia = inicioDia.AddDays(1);

        var snapshot = await _db.Collection("agendamentos")
            .WhereGreaterThanOrEqualTo("DataHora", Timestamp.FromDateTime(inicioDia.ToUniversalTime()))
            .WhereLessThan("DataHora", Timestamp.FromDateTime(fimDia.ToUniversalTime()))
            .OrderBy("DataHora")
            .GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ConvertTo<Agendamento>()).ToList();
    }

    // BARBEIROS
    public async Task<List<Barbeiro>> GetTodosBarbeirosAsync()
    {
        var snapshot = await _db.Collection("barbeiros").GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Barbeiro>()).ToList();
    }

    public async Task AddBarbeiroAsync(Barbeiro barbeiro)
    {
        await _db.Collection("barbeiros").AddAsync(barbeiro);
    }

    public async Task AtualizarStatusBarbeiroAsync(string id, bool ativo)
    {
        await _db.Collection("barbeiros").Document(id)
            .UpdateAsync("Ativo", ativo);
    }


    // SERVIÇOS
    public async Task<List<Servico>> GetTodosServicosAsync()
    {
        var snapshot = await _db.Collection("servicos").GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<Servico>()).ToList();
    }

    public async Task AddServicoAsync(Servico servico)
    {
        await _db.Collection("servicos").AddAsync(servico);
    }

    public async Task AtualizarStatusServicoAsync(string id, bool ativo)
    {
        await _db.Collection("servicos").Document(id)
            .UpdateAsync("Ativo", ativo);
    }
}