using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ISubmissionService, SubmissionService>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });
builder.Services.AddHttpClient<IJudge0Service, Judge0Service>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddHttpClient<IJudge0Service, Judge0Service>((sp, client) =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("Judge0");
    client.BaseAddress = new Uri(config["BaseUrl"]!);
    client.DefaultRequestHeaders.Add("X-RapidAPI-Key", config["ApiKey"]);
    client.DefaultRequestHeaders.Add("X-RapidAPI-Host", config["ApiHost"]);
});





builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Vytvoøení databáze pokud neexistuje
    context.Database.EnsureCreated();

    // Nebo použijte migrace v produkci:
    // context.Database.Migrate();
}
app.Run();
