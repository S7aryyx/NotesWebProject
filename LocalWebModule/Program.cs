using LocalJsonModule.Interfaces;
using LocalJsonModule.Repositories;
using LocalJsonModule.Services;
using LocalWebModule.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//
builder.Services.AddSingleton<IDataPathProvider, AppDataProvider>();
builder.Services.AddScoped<IUserJsonService, UserJsonRepository>();
builder.Services.AddScoped<INoteJsonService, NoteJsonRepository>();
//

//AddSingleton -> указатель на комопнент(ресурс) , который будет использовать
//программа , в течении всей своей жизни (пока запущена).

//AddScoped -> указатель на компонент(ресурс) , который будет использовать программа
//НО , с каждым новым и последующим вызовом N методов , данный файл пересоздаётся.

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
