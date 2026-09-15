using LocalJsonModule.Data;
using LocalJsonModule.Repositories;
using LocalJsonModule.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDataPathProvider, DataPathProvider>();

builder.Services.AddScoped<IUserRepository, UserJsonRepository>();
builder.Services.AddScoped<IFolderRepository, FolderJsonRepository>();
builder.Services.AddScoped<INoteRepository, NoteJsonRepository>();

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
