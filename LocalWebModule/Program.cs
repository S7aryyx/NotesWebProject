using LocalJsonModule.Interfaces;
using LocalJsonModule.Repositories.Json;
using LocalJsonModule.Services;
using LocalWebModule.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IDataPathProvider, AppDataProvider>();
builder.Services.AddScoped<IUserJsonService, UserJsonRepository>();
builder.Services.AddScoped<INoteJsonService, NoteJsonRepository>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();