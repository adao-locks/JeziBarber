using Google.Cloud.Firestore;

[FirestoreData]
public class Barbeiro
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public string Nome { get; set; }

    [FirestoreProperty]
    public bool Ativo { get; set; }
}