using Google.Cloud.Firestore;

[FirestoreData]
public class Servico
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public string Descricao { get; set; }

    [FirestoreProperty]
    public int DuracaoMinutos { get; set; }

    [FirestoreProperty]
    public double Valor { get; set; }

    [FirestoreProperty]
    public bool Ativo { get; set; }
}