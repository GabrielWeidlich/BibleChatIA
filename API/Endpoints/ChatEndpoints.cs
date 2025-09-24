using BibleChat.Api.DTOs;
using BibleChat.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibleChat.Api.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/explicar", async (
            [FromBody] PerguntaRequest request,
            [FromHeader(Name = "X-Session-Id")] string? sessionId, // Recebe o ID da sessão do header
            IExplicacaoService explicacaoService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Pergunta))
            {
                return Results.BadRequest(new { message = "A pergunta não pode estar vazia." });
            }

            // Gera um novo ID de sessão se nenhum for fornecido
            var currentSessionId = string.IsNullOrEmpty(sessionId) ? Guid.NewGuid().ToString() : sessionId;

            var resposta = await explicacaoService.ObterExplicacaoAsync(currentSessionId, request.Pergunta);

            // Retorna o ID da sessão no header da resposta para o cliente usar nas próximas requisições
            return Results.Ok(new { resposta, sessionId = currentSessionId });
        })
        .WithName("ExplicarConceitoBiblico")
        .WithSummary("Recebe uma pergunta e um sessionId e retorna uma explicação contextual.")
        .WithOpenApi();
    }
}