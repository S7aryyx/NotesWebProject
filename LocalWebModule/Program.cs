using LocalJsonModule.DataPathProvider;
using LocalJsonModule.Repositories;
using LocalJsonModule.Services;

var builder = WebApplication.CreateBuilder();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDataPathProvider, DataPathProvider>();
builder.Services.AddScoped<IUserRepository, UserJsonRepository>();
builder.Services.AddScoped<INoteRepository, NoteJsonRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<INoteService, NoteService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
