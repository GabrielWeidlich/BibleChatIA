namespace BibleChat.Api.Services;

// Define um objeto simples para representar uma mensagem
public record ChatMessage(string Role, string Text);

public interface IChatHistoryService
{
    // Adiciona uma mensagem ao histórico de uma sessão específica
    void AddMessage(string sessionId, ChatMessage message);
    
    // Obtém o histórico completo de uma sessão
    List<ChatMessage> GetHistory(string sessionId);
}