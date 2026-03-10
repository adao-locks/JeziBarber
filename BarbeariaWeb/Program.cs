using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";
    });

builder.Services.AddAuthorization();

var firebaseJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");

FirestoreDb firestoreDb;

if (!string.IsNullOrEmpty(firebaseJson))
{
    // Produção: lê as credenciais da variável de ambiente (Render)
    var credential = GoogleCredential.FromJson(firebaseJson);
    var firestoreClientBuilder = new FirestoreClientBuilder { Credential = credential };
    firestoreDb = await FirestoreDb.CreateAsync(
        "barbearia-agendamento-d2e93",
        firestoreClientBuilder.Build()
    );
    builder.Services.AddSingleton(firestoreDb);
}
else
{
    // Desenvolvimento local: usa o arquivo JSON normalmente
    Environment.SetEnvironmentVariable(
        "GOOGLE_APPLICATION_CREDENTIALS",
        Path.Combine(builder.Environment.ContentRootPath, "Config/barbearia-agendamento-d2e93-firebase-adminsdk-fbsvc-8938470cdf.json")
    );
    builder.Services.AddSingleton(provider => {
        return FirestoreDb.Create("barbearia-agendamento-d2e93");
    });
}

builder.Services.AddScoped<FirebaseService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IPasswordHasher<object>, PasswordHasher<object>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   
app.UseAuthorization();    

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Agendamento}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();