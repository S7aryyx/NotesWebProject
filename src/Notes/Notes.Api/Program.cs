using LocalJsonModule.Data;
using LocalJsonModule.Repositories;
using LocalJsonModule.Services;
using LocalJsonModule.Services.Auth;
using LocalJsonModule.Services.Register;
using LocalJsonModule.Services.Update;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using LocalJsonModule.Models;
using LocalJsonModule.Repositories.Json;
using NotesWebProject.Repositories.PostgreSQL;
using LocalJsonModule.Repositories.PostgreSQL;
using LocalWebModule.Controllers;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();

    options.ForwardLimit = 1;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Notes.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpClient("YooKassa");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDataPathProvider, DataPathProvider>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUpdateService, UpdateService>();

//Json API's
//
//builder.Services.AddScoped<IUserRepository, UserJsonRepository>();
//builder.Services.AddScoped<IFolderRepository, FolderJsonRepository>();
//builder.Services.AddScoped<INoteRepository, NoteJsonRepository>();
//

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<INoteService, NoteService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Не задана строка подключения ConnectionStrings:DefaultConnection. Проверь User Secrets для Notes.Api.");
}

builder.Services.AddScoped<IConnectionFactory>(_ => new DbConnectionFactory(connectionString));
builder.Services.AddScoped<IUserRepository, UserPostgresRepository>();
builder.Services.AddScoped<INoteRepository, NotePostgresRepository>();
builder.Services.AddScoped<IFolderRepository, FolderPostgresRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseForwardedHeaders();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
