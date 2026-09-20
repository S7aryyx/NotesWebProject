using LocalJsonModule.Data;
using LocalJsonModule.Repositories;
using LocalJsonModule.Services;
using LocalJsonModule.Services.Auth;
using LocalJsonModule.Services.Register;
using LocalJsonModule.Services.Update;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDataPathProvider, DataPathProvider>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUpdateService, UpdateService>();
builder.Services.AddScoped<IUserRepository, UserJsonRepository>();
builder.Services.AddScoped<IFolderRepository, FolderJsonRepository>();
builder.Services.AddScoped<INoteRepository, NoteJsonRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFolderService, FolderService>();
builder.Services.AddScoped<INoteService, NoteService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();

app.Run();
