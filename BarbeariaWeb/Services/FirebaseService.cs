using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

public class FirebaseService
{
    private readonly FirestoreDb _db;

    public FirebaseService(FirestoreDb db)
    {
        //Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("123"));
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

    public async Task AtualizarStatusAsync(string id, string status)
    {
        var docRef = _db.Collection("agendamentos").Document(id);

        await docRef.UpdateAsync("Status", status);
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
        var inicioLocal = data.Date;
        var fimLocal = inicioLocal.AddDays(1);

        var inicioUtc = DateTime.SpecifyKind(inicioLocal, DateTimeKind.Local).ToUniversalTime();
        var fimUtc = DateTime.SpecifyKind(fimLocal, DateTimeKind.Local).ToUniversalTime();

        var snapshot = await _db.Collection("agendamentos")
            .WhereGreaterThanOrEqualTo("DataHora", Timestamp.FromDateTime(inicioUtc))
            .WhereLessThan("DataHora", Timestamp.FromDateTime(fimUtc))
            .OrderBy("DataHora")
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(d => d.ConvertTo<Agendamento>())
            .ToList();
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

    public async Task<Admin?> GetAdminPorUsuarioAsync(string usuario)
    {
        var snapshot = await _db.Collection("admins")
                                .WhereEqualTo("usuario", usuario)
                                .GetSnapshotAsync();

        var doc = snapshot.Documents.FirstOrDefault();

        if (doc == null)
            return null;

        var admin = doc.ConvertTo<Admin>();

        admin.Id = doc.Id;

        return admin;
    }

    public async Task AlterarSenha(string adminId, string senhaAtual, string novaSenha)
    {
        if (string.IsNullOrEmpty(adminId))
            throw new Exception("Admin inválido.");

        var doc = await _db.Collection("admins").Document(adminId).GetSnapshotAsync();

        if (!doc.Exists)
            throw new Exception("Usuário não encontrado.");

        var senhaHash = doc.GetValue<string>("senhaHash");

        if (!BCrypt.Net.BCrypt.Verify(senhaAtual, senhaHash))
            throw new Exception("Senha atual incorreta.");

        var novoHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);

        await doc.Reference.UpdateAsync("senhaHash", novoHash);
    }

    public async Task<List<Agendamento>> GetAgendamentosPorMesAsync(DateTime data)
    {
        var inicioMes = new DateTime(data.Year, data.Month, 1);
        var fimMes = inicioMes.AddMonths(1);

        var inicioUtc = DateTime.SpecifyKind(inicioMes, DateTimeKind.Local).ToUniversalTime();
        var fimUtc = DateTime.SpecifyKind(fimMes, DateTimeKind.Local).ToUniversalTime();

        var snapshot = await _db.Collection("agendamentos")
            .WhereGreaterThanOrEqualTo("DataHora", Timestamp.FromDateTime(inicioUtc))
            .WhereLessThan("DataHora", Timestamp.FromDateTime(fimUtc))
            .OrderBy("DataHora")
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(d => d.ConvertTo<Agendamento>())
            .ToList();
    }
}