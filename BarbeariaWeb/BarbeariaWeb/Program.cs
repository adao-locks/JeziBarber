using Google.Cloud.Firestore;

var builder = WebApplication.CreateBuilder(args);

Environment.SetEnvironmentVariable(
    "GOOGLE_APPLICATION_CREDENTIALS", 
    Path.Combine(builder.Environment.ContentRootPath, "Config/barbearia-agendamento-d2e93-firebase-adminsdk-fbsvc-8938470cdf.json")
);

builder.Services.AddSingleton(provider =>
{
    return FirestoreDb.Create("barbearia-agendamento-d2e93");
});

builder.Services.AddScoped<FirebaseService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Agendamento}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
