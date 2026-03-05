using Google.Cloud.Firestore;

[FirestoreData]
public class Admin
{
    [FirestoreProperty("usuario")]
    public string usuario { get; set; }

    [FirestoreProperty("senhaHash")]
    public string senhaHash { get; set; }

    [FirestoreProperty("ativo")]
    public bool ativo { get; set; }
}