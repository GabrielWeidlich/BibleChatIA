namespace BibleChat.Api.Services;

public interface IExplicacaoService
{
    /// <summary>
    /// Obtém uma explicação de um modelo de linguagem generativa com base em uma pergunta.
    /// </summary>
    /// <param name="pergunta">A pergunta do usuário.</param>
    /// <returns>A explicação gerada pelo modelo.</returns>
    Task<string> ObterExplicacaoAsync(string sessionId, string pergunta);
}