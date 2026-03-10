using Google.Cloud.Firestore;

[FirestoreData]
public class Agendamento
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public string NomeCliente { get; set; }

    [FirestoreProperty]
    public string Telefone { get; set; }

    [FirestoreProperty]
    public string BarbeiroId { get; set; }

    [FirestoreProperty]
    public string ServicoId { get; set; }

    [FirestoreProperty]
    public Timestamp DataHora { get; set; }

    [FirestoreProperty]
    public string Status { get; set; }
}