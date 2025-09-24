using System.Collections.Concurrent;

namespace BibleChat.Api.Services;

public class InMemoryChatHistoryService : IChatHistoryService
{
    // Usamos um ConcurrentDictionary para ser thread-safe.
    // A chave é o ID da sessão, e o valor é a lista de mensagens.
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _histories = new();

    public void AddMessage(string sessionId, ChatMessage message)
    {
        // Obtém a lista de mensagens para a sessão ou cria uma nova se não existir
        var history = _histories.GetOrAdd(sessionId, new List<ChatMessage>());
        history.Add(message);
    }

    public List<ChatMessage> GetHistory(string sessionId)
    {
        // Retorna o histórico da sessão ou uma lista vazia se não houver
        return _histories.TryGetValue(sessionId, out var history) ? history : new List<ChatMessage>();
    }
}