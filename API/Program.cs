using BibleChat.Api.Endpoints;
using BibleChat.Api.Services;
using DotNetEnv; // Adicionado para carregar o arquivo .env

// Carrega as variáveis de ambiente do arquivo .env no início da aplicação
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// --- Configuração da Injeção de Dependência ---

// Adiciona o IHttpClientFactory para ser usado nos serviços
builder.Services.AddHttpClient();

// Adiciona os serviços da aplicação
builder.Services.AddScoped<IExplicacaoService, GeminiService>();
builder.Services.AddSingleton<IChatHistoryService, InMemoryChatHistoryService>();
builder.Services.AddLogging();

// Adiciona serviços para a documentação da API (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configura a política de CORS para permitir requisições do front-end
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000") // URL do seu front-end
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// --- Configuração do Pipeline de Requisições HTTP ---

// Habilita o Swagger apenas em ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Removido app.UseHttpsRedirection(); para evitar problemas no Docker.
// A segurança (HTTPS) geralmente é gerenciada por um proxy reverso.

// Habilita a política de CORS
app.UseCors();

// Mapeia os endpoints da aplicação (ex: /explicar)
app.MapChatEndpoints();

app.Run();
