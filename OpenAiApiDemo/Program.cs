using OpenAiApiDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// Habilitar controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

// Inyectar el servicio personalizado de OpenAI
builder.Services.AddHttpClient<OpenAiService>();
builder.Services.AddHttpClient<GeminiService>();

var app = builder.Build();

// Configurar middleware y Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.RoutePrefix = "swagger";
    });
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthorization();
app.MapControllers();

app.Run();
