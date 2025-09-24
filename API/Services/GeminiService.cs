using Markdig;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BibleChat.Api.Services;

public class GeminiService : IExplicacaoService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;
    private readonly IChatHistoryService _chatHistoryService; // Adicionado
    private const string GeminiApiUrlBase = "https://generativelanguage.googleapis.com/v1beta/models/";
    private const string ModelName = "gemini-1.5-flash";

    // Injeta o serviço de histórico
    public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GeminiService> logger, IChatHistoryService chatHistoryService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _chatHistoryService = chatHistoryService; // Adicionado
    }

    // A assinatura do método agora precisa do sessionId
    public async Task<string> ObterExplicacaoAsync(string sessionId, string pergunta)
    {
        var apiKey = _configuration["GEMINI_API_KEY"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("API Key do Gemini não encontrada.");
            throw new InvalidOperationException("API Key do Gemini não configurada.");
        }

        try
        {
            // Adiciona a pergunta atual do usuário ao histórico
            _chatHistoryService.AddMessage(sessionId, new ChatMessage("user", pergunta));

            // Obtém todo o histórico da conversa
            var history = _chatHistoryService.GetHistory(sessionId);

            // Converte o histórico para o formato que a API do Gemini espera
            var geminiContents = history.Select(msg => new
            {
                role = msg.Role,
                parts = new[] { new { text = msg.Text } }
            }).ToList();

            var systemPromptMarkdown = await File.ReadAllTextAsync("Prompts/SystemPrompt.md");
            var systemPromptText = Markdown.ToPlainText(systemPromptMarkdown);

            var requestBody = new
            {
                contents = geminiContents, // Usa o histórico completo
                system_instruction = new
                {
                    parts = new[] { new { text = systemPromptText } }
                }
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();
            var apiUrl = $"{GeminiApiUrlBase}{ModelName}:generateContent?key={apiKey}";
            
            _logger.LogInformation("Enviando requisição para a API do Gemini para a sessão {SessionId}", sessionId);

            var response = await httpClient.PostAsync(apiUrl, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro na API do Gemini. Status: {StatusCode}, Resposta: {ErrorContent}", response.StatusCode, errorContent);
                return "Desculpe, não consegui processar sua pergunta no momento.";
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var parsedResponse = JsonNode.Parse(jsonResponse);
            var generatedText = parsedResponse?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.GetValue<string>() ?? string.Empty;

            if (!string.IsNullOrEmpty(generatedText))
            {
                // Adiciona a resposta do modelo ao histórico para a próxima interação
                _chatHistoryService.AddMessage(sessionId, new ChatMessage("model", generatedText));
            }

            return generatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado na comunicação com o Gemini.");
            throw;
        }
    }
}