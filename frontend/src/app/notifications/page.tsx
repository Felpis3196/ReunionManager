'use client';

import React, { useEffect, useState } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import Layout from '@/components/Layout/Layout';
import { useAuthStore } from '@/stores/authStore';
import { notificationsService, authService, type NotificationsResponse, type InviteResponse } from '@/services/api';

export default function NotificationsPage() {
  const router = useRouter();
  const { user, refreshToken } = useAuthStore();
  const [data, setData] = useState<NotificationsResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [acceptingId, setAcceptingId] = useState<string | null>(null);
  const [invitePassword, setInvitePassword] = useState('');
  const [acceptError, setAcceptError] = useState<string | null>(null);

  useEffect(() => {
    const timer = setTimeout(() => {
      if (user == null) {
        router.replace('/login');
        return;
      }
      loadNotifications();
    }, 100);
    return () => clearTimeout(timer);
  }, [user]);

  const loadNotifications = async () => {
    if (!user) return;
    try {
      setIsLoading(true);
      setError(null);
      const result = await notificationsService.getAll();
      setData(result);
    } catch (e: unknown) {
      console.error('Error loading notifications:', e);
      const err = e as { response?: { status?: number; data?: { error?: string; message?: string } } };
      const status = err.response?.status;
      const body = err.response?.data;
      const apiMessage = body?.error ?? body?.message;
      const text =
        apiMessage
        ?? (status === 401 ? 'Sessao expirada. Faca login novamente.' : undefined)
        ?? (status === 403 ? 'Voce nao tem permissao para ver notificacoes.' : undefined)
        ?? (status != null && status >= 500 ? 'Erro temporario no servidor. Tente mais tarde.' : undefined)
        ?? 'Nao foi possivel carregar as notificacoes.';
      setError(text);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAcceptInvite = async (invite: InviteResponse) => {
    setAcceptError(null);
    const code = invite.inviteCode ?? (invite as any).InviteCode ?? '';
    const hasPwd = invite.hasPassword ?? (invite as any).HasPassword;
    if (hasPwd && !invitePassword.trim()) {
      setAcceptingId(invite.id);
      return;
    }
    try {
      await authService.acceptInvite({
        inviteCode: code,
        ...(hasPwd && invitePassword.trim() ? { invitePassword: invitePassword.trim() } : {}),
      });

      // Atualiza token e usuario (org/cargo/permissoes) imediatamente
      await refreshToken();

      setInvitePassword('');
      setAcceptingId(null);
      loadNotifications();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setAcceptError(err.response?.data?.error || 'Erro ao aceitar convite.');
    }
  };

  const meetingsStartingSoon = data?.meetingsStartingSoon ?? (data as any)?.MeetingsStartingSoon ?? [];
  const invites = (data?.invites ?? (data as any)?.Invites ?? []) as InviteResponse[];
  const upcomingMeetings = data?.upcomingMeetings ?? (data as any)?.UpcomingMeetings ?? [];
  const tasksDueSoon = data?.tasksDueSoon ?? (data as any)?.TasksDueSoon ?? [];
  const hasAny = data && (
    meetingsStartingSoon.length > 0 ||
    invites.length > 0 ||
    upcomingMeetings.length > 0 ||
    tasksDueSoon.length > 0
  );

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-3xl mx-auto">
          <div className="mb-8">
            <h1 className="page-title">Notificacoes</h1>
            <p className="text-muted mt-1">
              Convites, reunioes proximas e tarefas a vencer
            </p>
          </div>

          {error && (
            <div className="mb-6 p-4 rounded-xl border bg-red-50 border-red-200 text-red-700 flex items-center gap-2">
              <svg className="w-5 h-5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span className="text-sm font-medium">{error}</span>
            </div>
          )}

          {isLoading && (
            <div className="card divide-y divide-gray-100">
              {[1, 2, 3].map((i) => (
                <div key={i} className="p-4 flex items-center gap-4">
                  <div className="w-10 h-10 skeleton rounded-full" />
                  <div className="flex-1">
                    <div className="h-4 skeleton w-2/3 mb-2" />
                    <div className="h-3 skeleton w-1/3" />
                  </div>
                </div>
              ))}
            </div>
          )}

          {!isLoading && data && (
            <>
              {!hasAny && (
                <div className="card p-8 text-center">
                  <div className="w-14 h-14 bg-gray-100 dark:bg-slate-700 rounded-full flex items-center justify-center mx-auto mb-4">
                    <svg className="w-7 h-7 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                    </svg>
                  </div>
                  <p className="text-gray-500">Nenhuma notificacao no momento.</p>
                  <p className="text-sm text-gray-400 mt-1">Convites, reunioes e tarefas aparecerao aqui.</p>
                </div>
              )}

              {meetingsStartingSoon.length > 0 && (
                <section className="card mb-6 animate-fadeIn border-2 border-amber-200 bg-amber-50/50">
                  <div className="px-6 py-4 border-b border-amber-200/50 flex items-center gap-2">
                    <div className="w-8 h-8 bg-amber-100 rounded-lg flex items-center justify-center">
                      <svg className="w-4 h-4 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                      </svg>
                    </div>
                    <h2 className="font-semibold text-amber-900">Reunioes nos proximos 30 min</h2>
                  </div>
                  <ul className="divide-y divide-amber-100">
                    {meetingsStartingSoon.map((m: { id: string; title: string; scheduledAt: string; type: string }) => (
                      <li key={m.id} className="p-4 hover:bg-amber-50/50 transition-colors">
                        <Link href={`/meetings/${m.id}`} className="block">
                          <p className="font-medium text-gray-900">{m.title}</p>
                          <p className="text-sm text-amber-700 mt-0.5">
                            {new Date(m.scheduledAt).toLocaleString('pt-BR', {
                              dateStyle: 'short',
                              timeStyle: 'short',
                            })}{' '}
                            · {m.type}
                          </p>
                          <span className="inline-flex items-center gap-1 mt-2 text-sm font-medium text-amber-700">
                            Entrar na reuniao
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                            </svg>
                          </span>
                        </Link>
                      </li>
                    ))}
                  </ul>
                </section>
              )}

              {invites.length > 0 && (
                <section className="card mb-6 animate-fadeIn">
                  <div className="px-6 py-4 border-b border-gray-100 flex items-center gap-2">
                    <div className="w-8 h-8 bg-indigo-100 rounded-lg flex items-center justify-center">
                      <svg className="w-4 h-4 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                      </svg>
                    </div>
                    <h2 className="font-semibold text-gray-900">Convites para equipe</h2>
                  </div>
                  <ul className="divide-y divide-gray-100">
                    {invites.map((invite) => {
                      const code = invite.inviteCode ?? (invite as any).InviteCode ?? '';
                      return (
                      <li key={invite.id} className="p-4 hover:bg-gray-50 transition-colors">
                        <div>
                          <p className="font-medium text-gray-900">Voce foi convidado para uma equipe</p>
                          <p className="text-sm text-gray-500 mt-0.5">
                            Codigo: {code}
                            {(invite.hasPassword ?? (invite as any).HasPassword) && (
                              <span className="ml-2 px-2 py-0.5 rounded-full text-xs bg-slate-100 text-slate-600">
                                Convite protegido por senha
                              </span>
                            )}
                          </p>
                          <p className="text-xs text-gray-400 mt-1">
                            Expira em {new Date(invite.expiresAt).toLocaleDateString('pt-BR')}
                          </p>
                          {acceptingId === invite.id ? (
                            <div className="mt-3 space-y-2">
                              {(invite.hasPassword ?? (invite as any).HasPassword) && (
                                <input
                                  type="password"
                                  value={invitePassword}
                                  onChange={(e) => setInvitePassword(e.target.value)}
                                  placeholder="Senha do convite"
                                  className="block w-full rounded-lg border border-gray-200 px-3 py-2 text-sm"
                                />
                              )}
                              {acceptError && <p className="text-sm text-red-600">{acceptError}</p>}
                              <div className="flex gap-2">
                                <button
                                  type="button"
                                  onClick={() => handleAcceptInvite(invite)}
                                  className="btn-primary text-sm py-1.5 px-3"
                                >
                                  Confirmar
                                </button>
                                <button
                                  type="button"
                                  onClick={() => { setAcceptingId(null); setInvitePassword(''); setAcceptError(null); }}
                                  className="btn-secondary text-sm py-1.5 px-3"
                                >
                                  Cancelar
                                </button>
                              </div>
                            </div>
                          ) : (
                            <div className="mt-2 flex items-center gap-2">
                              <button
                                type="button"
                                onClick={() => handleAcceptInvite(invite)}
                                className="inline-flex items-center gap-1 text-sm font-medium text-indigo-600 hover:text-indigo-700"
                              >
                                Aceitar convite
                                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                                </svg>
                              </button>
                              <span className="text-gray-300">|</span>
                              <Link
                                href={`/register?invite=${encodeURIComponent(code)}`}
                                className="text-sm text-gray-500 hover:text-gray-700"
                              >
                                Criar conta com este codigo
                              </Link>
                            </div>
                          )}
                        </div>
                      </li>
                    ); })}
                  </ul>
                </section>
              )}

              {upcomingMeetings.length > 0 && (
                <section className="card mb-6 animate-fadeIn">
                  <div className="px-6 py-4 border-b border-gray-100 flex items-center gap-2">
                    <div className="w-8 h-8 bg-emerald-100 rounded-lg flex items-center justify-center">
                      <svg className="w-4 h-4 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                      </svg>
                    </div>
                    <h2 className="font-semibold text-gray-900">Reunioes proximas</h2>
                  </div>
                  <ul className="divide-y divide-gray-100">
                    {upcomingMeetings.map((m: { id: string; title: string; scheduledAt: string; type: string }) => (
                      <li key={m.id} className="p-4 hover:bg-gray-50 transition-colors">
                        <Link href={`/meetings/${m.id}`} className="block">
                          <p className="font-medium text-gray-900">{m.title}</p>
                          <p className="text-sm text-gray-500 mt-0.5">
                            {new Date(m.scheduledAt).toLocaleString('pt-BR', {
                              dateStyle: 'short',
                              timeStyle: 'short',
                            })}{' '}
                            · {m.type}
                          </p>
                          <span className="inline-flex items-center gap-1 mt-2 text-sm font-medium text-indigo-600">
                            Ver reuniao
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                            </svg>
                          </span>
                        </Link>
                      </li>
                    ))}
                  </ul>
                </section>
              )}

              {tasksDueSoon.length > 0 && (
                <section className="card mb-6 animate-fadeIn">
                  <div className="px-6 py-4 border-b border-gray-100 flex items-center gap-2">
                    <div className="w-8 h-8 bg-amber-100 rounded-lg flex items-center justify-center">
                      <svg className="w-4 h-4 text-amber-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
                      </svg>
                    </div>
                    <h2 className="font-semibold text-gray-900">Tarefas a vencer</h2>
                  </div>
                  <ul className="divide-y divide-gray-100">
                    {tasksDueSoon.map((t: { id: string; title: string; dueDate?: string; status: string; priority: string; meetingId: string }) => (
                      <li key={t.id} className="p-4 hover:bg-gray-50 transition-colors">
                        <Link href={`/tasks${t.meetingId ? `?meetingId=${t.meetingId}` : ''}`} className="block">
                          <p className="font-medium text-gray-900">{t.title}</p>
                          <p className="text-sm text-gray-500 mt-0.5">
                            {t.dueDate
                              ? `Vence em ${new Date(t.dueDate).toLocaleDateString('pt-BR')}`
                              : 'Sem data'}{' '}
                            · {t.priority} · {t.status}
                          </p>
                          <span className="inline-flex items-center gap-1 mt-2 text-sm font-medium text-indigo-600">
                            Ver tarefas
                            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
                            </svg>
                          </span>
                        </Link>
                      </li>
                    ))}
                  </ul>
                </section>
              )}
            </>
          )}
        </div>
      </div>
    </Layout>
  );
}
