'use client';

import React, { useEffect, useState, useRef, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/components/Layout/Layout';
import { useAuthStore } from '@/stores/authStore';
import { chatService, getChatHubUrl, type ChatMessageDto } from '@/services/api';
import * as SignalR from '@microsoft/signalr';

function getInitials(name: string) {
  return name
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2);
}

function formatTime(iso: string) {
  try {
    const d = new Date(iso);
    return d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
  } catch {
    return '';
  }
}

export default function ChatPage() {
  const router = useRouter();
  const { user, accessToken } = useAuthStore();
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [inputText, setInputText] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [connectionState, setConnectionState] = useState<string>('Disconnected');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const connectionRef = useRef<SignalR.HubConnection | null>(null);

  const scrollToBottom = useCallback(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, []);

  useEffect(() => {
    if (!user?.organizationId) {
      router.replace('/hub');
      return;
    }
  }, [user?.organizationId, router]);

  useEffect(() => {
    if (!user?.organizationId || !accessToken) return;

    let mounted = true;
    const token = accessToken;

    (async () => {
      try {
        const list = await chatService.getMessages(50);
        if (mounted) setMessages(Array.isArray(list) ? list : []);
      } catch (e) {
        console.error('Failed to load chat messages', e);
        if (mounted) setError('Erro ao carregar mensagens.');
      } finally {
        if (mounted) setIsLoading(false);
      }
    })();

    const hubUrl = getChatHubUrl();
    const urlWithToken = `${hubUrl}?access_token=${encodeURIComponent(token)}`;
    const connection = new SignalR.HubConnectionBuilder()
      .withUrl(urlWithToken, { skipNegotiation: false, withCredentials: true })
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveMessage', (msg: ChatMessageDto) => {
      if (!mounted) return;
      setMessages((prev) => {
        if (prev.some((m) => m.id === msg.id)) return prev;
        return [...prev, msg];
      });
    });

    connection.onreconnecting(() => setConnectionState('Reconnecting'));
    connection.onreconnected(() => setConnectionState('Connected'));
    connection.onclose(() => setConnectionState('Disconnected'));

    connection
      .start()
      .then(() => {
        if (mounted) setConnectionState('Connected');
      })
      .catch((err) => {
        console.error('SignalR connection error', err);
        if (mounted) setConnectionState('Error');
      });

    connectionRef.current = connection;

    return () => {
      mounted = false;
      connection.stop().catch(() => {});
      connectionRef.current = null;
    };
  }, [user?.organizationId, accessToken]);

  useEffect(() => {
    scrollToBottom();
  }, [messages, scrollToBottom]);

  const handleSend = async () => {
    const text = inputText.trim();
    if (!text || isSending || !user?.organizationId) return;

    setIsSending(true);
    setInputText('');
    try {
      const sent = await chatService.sendMessage(text);
      setMessages((prev) => {
        if (prev.some((m) => m.id === sent.id)) return prev;
        return [...prev, sent];
      });
    } catch (e) {
      console.error('Send message failed', e);
      setInputText(text);
      setError('Erro ao enviar mensagem.');
    } finally {
      setIsSending(false);
    }
  };

  if (!user?.organizationId) {
    return (
      <Layout>
        <div className="min-h-screen flex items-center justify-center">
          <p className="text-gray-500">Redirecionando...</p>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-3xl mx-auto">
          <div className="flex items-center justify-between mb-4">
            <h1 className="page-title">Chat da equipe</h1>
            <span
              className={`text-xs px-2 py-1 rounded-full ${
                connectionState === 'Connected'
                  ? 'bg-emerald-100 text-emerald-700'
                  : connectionState === 'Reconnecting'
                  ? 'bg-amber-100 text-amber-700'
                  : 'bg-gray-100 text-gray-600'
              }`}
            >
              {connectionState === 'Connected' ? 'Ao vivo' : connectionState}
            </span>
          </div>

          {error && (
            <div className="mb-4 p-3 rounded-lg bg-red-50 border border-red-200 text-red-700 text-sm">
              {error}
            </div>
          )}

          <div className="card overflow-hidden flex flex-col" style={{ height: '60vh' }}>
            {isLoading ? (
              <div className="flex-1 flex items-center justify-center text-gray-500">Carregando...</div>
            ) : (
              <div className="flex-1 overflow-y-auto p-4 space-y-4">
                {messages.length === 0 ? (
                  <p className="text-center text-gray-500 text-sm">Nenhuma mensagem ainda. Seja o primeiro a enviar.</p>
                ) : (
                  messages.map((msg) => (
                    <div key={msg.id} className="flex gap-3">
                      {msg.userAvatarUrl ? (
                        <img
                          src={msg.userAvatarUrl}
                          alt={msg.userName}
                          className="w-9 h-9 rounded-full object-cover flex-shrink-0"
                        />
                      ) : (
                        <div className="w-9 h-9 rounded-full bg-gradient-to-br from-indigo-500 to-violet-500 flex items-center justify-center text-white text-sm font-medium flex-shrink-0">
                          {getInitials(msg.userName || '?')}
                        </div>
                      )}
                      <div className="min-w-0 flex-1">
                        <div className="flex items-baseline gap-2 flex-wrap">
                          <span className="font-medium text-gray-900 text-sm">{msg.userName || 'Usuario'}</span>
                          <span className="text-xs text-gray-400">{formatTime(msg.createdAt)}</span>
                        </div>
                        <p className="text-gray-700 text-sm mt-0.5 break-words">{msg.text}</p>
                      </div>
                    </div>
                  ))
                )}
                <div ref={messagesEndRef} />
              </div>
            )}

            <div className="p-4 border-t border-gray-100 flex gap-2">
              <input
                type="text"
                value={inputText}
                onChange={(e) => setInputText(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && !e.shiftKey && handleSend()}
                placeholder="Digite uma mensagem..."
                className="flex-1 rounded-lg border border-gray-200 px-4 py-2 text-sm focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                disabled={isSending || connectionState !== 'Connected'}
              />
              <button
                type="button"
                onClick={handleSend}
                disabled={!inputText.trim() || isSending || connectionState !== 'Connected'}
                className="btn-primary px-4 py-2"
              >
                {isSending ? 'Enviando...' : 'Enviar'}
              </button>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}
