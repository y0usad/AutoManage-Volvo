using Microsoft.EntityFrameworkCore;
using AutoManage.Data;

var builder = WebApplication.CreateBuilder(args);

// Adicionar DbContext com SQL Server
builder.Services.AddDbContext<AutoManageContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adicionar controllers
builder.Services.AddControllers();

// Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AutoManage API",
        Version = "v1",
        Description = "API para gerenciamento de concessionárias de veículos",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "AutoManage System"
        }
    });
});

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoManage API v1");
        c.RoutePrefix = string.Empty; // Swagger na raiz
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

