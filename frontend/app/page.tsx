"use client";

import { useState, FormEvent, useRef, useEffect } from 'react';
import axios from 'axios';
import ReactMarkdown from 'react-markdown';

// Define a estrutura de uma mensagem no chat
interface ChatMessage {
  role: 'user' | 'model';
  text: string;
}

export default function Home() {
  const [pergunta, setPergunta] = useState<string>('');
  const [chatHistory, setChatHistory] = useState<ChatMessage[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  const [sessionId, setSessionId] = useState<string | null>(null);
  const chatContainerRef = useRef<HTMLDivElement>(null);

  // Efeito para rolar para a última mensagem
  useEffect(() => {
    if (chatContainerRef.current) {
      chatContainerRef.current.scrollTop = chatContainerRef.current.scrollHeight;
    }
  }, [chatHistory]);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (!pergunta.trim() || isLoading) return;

    const novaPergunta: ChatMessage = { role: 'user', text: pergunta };
    setChatHistory(prev => [...prev, novaPergunta]);
    setPergunta('');
    setIsLoading(true);
    setError(null);

    try {
      const apiBaseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080';
      
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
      };

      if (sessionId) {
        headers['X-Session-Id'] = sessionId;
      }
      
      const response = await axios.post(`${apiBaseUrl}/explicar`, 
        { pergunta: pergunta },
        { headers: headers }
      );

      const novaResposta: ChatMessage = { role: 'model', text: response.data.resposta };
      setChatHistory(prev => [...prev, novaResposta]);

      if (response.data.sessionId) {
        setSessionId(response.data.sessionId);
      }

    } catch (err) {
      console.error("Erro ao buscar explicação:", err);
      setError("Não foi possível obter a resposta. Verifique a conexão com a API.");
      const erroResposta: ChatMessage = { role: 'model', text: "Desculpe, ocorreu um erro ao conectar com o servidor." };
      setChatHistory(prev => [...prev, erroResposta]);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <main className="flex h-screen flex-col items-center bg-gray-100">
      <div className="flex flex-col h-full w-full max-w-4xl bg-white shadow-lg">
        <header className="p-4 border-b border-gray-200">
          <h1 className="text-2xl font-bold text-stone-800">
            Bible Chat 📖
          </h1>
        </header>

        <div ref={chatContainerRef} className="flex-1 overflow-y-auto p-6 space-y-4">
          {chatHistory.map((msg, index) => (
            <div key={index} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
              <div className={`max-w-lg p-4 rounded-xl prose ${msg.role === 'user' ? 'bg-blue-600 text-white' : 'bg-gray-200 text-stone-800'}`}>
                <ReactMarkdown>{msg.text}</ReactMarkdown>
              </div>
            </div>
          ))}
           {isLoading && (
            <div className="flex justify-start">
              <div className="max-w-lg p-4 rounded-xl bg-gray-200 text-stone-800">
                <div className="flex items-center space-x-2">
                  <div className="w-2 h-2 bg-gray-500 rounded-full animate-pulse"></div>
                  <div className="w-2 h-2 bg-gray-500 rounded-full animate-pulse delay-75"></div>
                  <div className="w-2 h-2 bg-gray-500 rounded-full animate-pulse delay-150"></div>
                </div>
              </div>
            </div>
          )}
        </div>
        
        {error && (
          <div className="p-4 border-t border-gray-200">
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded-lg" role="alert">
              <strong>Erro: </strong>
              <span>{error}</span>
            </div>
          </div>
        )}

        <div className="p-4 border-t border-gray-200">
          <form onSubmit={handleSubmit} className="flex items-center gap-4">
            <input
              type="text"
              value={pergunta}
              onChange={(e) => setPergunta(e.target.value)}
              placeholder="Digite sua pergunta bíblica aqui..."
              className="flex-1 p-3 text-stone-800 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition"
              disabled={isLoading}
            />
            <button
              type="submit"
              className="bg-blue-600 text-white font-semibold py-3 px-5 rounded-lg hover:bg-blue-700 disabled:bg-blue-300 transition-colors duration-300"
              disabled={isLoading || !pergunta.trim()}
            >
              Enviar
            </button>
          </form>
        </div>
      </div>
    </main>
  );
}