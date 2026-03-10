'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/components/Layout/Layout';
import Link from 'next/link';
import { dashboardService, DashboardStats, ProductivityStats, authService, MyOrganizationItem } from '@/services/api';
import { useAuthStore } from '@/stores/authStore';

const typeLabels: Record<string, string> = {
  Planning: 'Planejamento',
  Review: 'Revisao',
  Standup: 'Daily',
  Retrospective: 'Retro',
  OneOnOne: '1:1',
  Other: 'Outro',
};

const priorityConfig: Record<string, { label: string; color: string }> = {
  Low: { label: 'Baixa', color: 'bg-gray-100 text-gray-700' },
  Medium: { label: 'Media', color: 'bg-blue-50 text-blue-700' },
  High: { label: 'Alta', color: 'bg-amber-50 text-amber-700' },
  Critical: { label: 'Critica', color: 'bg-red-50 text-red-700' },
};

const GENERAL_DASHBOARD_VALUE = '';

export default function DashboardPage() {
  const router = useRouter();
  const user = useAuthStore((s) => s.user);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const isSiteAdmin = user?.isSiteAdmin ?? false;
  const [organizations, setOrganizations] = useState<MyOrganizationItem[]>([]);
  const [selectedScope, setSelectedScope] = useState<string>(GENERAL_DASHBOARD_VALUE);
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [productivity, setProductivity] = useState<ProductivityStats | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Redirect to login only after giving auth store time to rehydrate from localStorage
  useEffect(() => {
    const t = setTimeout(() => {
      if (isAuthenticated === false) router.replace('/login');
    }, 100);
    return () => clearTimeout(t);
  }, [isAuthenticated, router]);

  // Wait for user (auth rehydration) before fetching orgs
  useEffect(() => {
    if (user == null) return;
    let mounted = true;
    (async () => {
      try {
        const orgs = await authService.getMyOrganizations();
        if (!mounted) return;
        setOrganizations(orgs);
        if (isSiteAdmin) {
          setSelectedScope(GENERAL_DASHBOARD_VALUE);
        } else if (orgs.length > 0) {
          const currentOrgId = user?.organizationId;
          const firstId = orgs[0].id;
          setSelectedScope(currentOrgId && orgs.some((o) => o.id === currentOrgId) ? currentOrgId : firstId);
        } else {
          setSelectedScope(GENERAL_DASHBOARD_VALUE);
          setError('Voce nao pertence a nenhuma organizacao. Aceite um convite em Notificacoes ou peca um link de convite ao seu gestor.');
          setIsLoading(false);
        }
      } catch (e) {
        if (!mounted) return;
        setError('Erro ao carregar organizacoes. Verifique se esta logado.');
        setIsLoading(false);
      }
    })();
    return () => { mounted = false; };
  }, [user?.id, isSiteAdmin, user?.organizationId]);

  useEffect(() => {
    if (user == null) return;
    if (organizations.length === 0 && !isSiteAdmin) return;
    loadData(selectedScope === GENERAL_DASHBOARD_VALUE ? undefined : selectedScope);
  }, [user?.id, selectedScope, organizations.length, isSiteAdmin]);

  const loadData = async (organizationId?: string) => {
    try {
      setIsLoading(true);
      setError(null);
      const [statsData, productivityData] = await Promise.all([
        dashboardService.getStats(organizationId),
        dashboardService.getProductivity(30, organizationId),
      ]);
      setStats(statsData);
      setProductivity(productivityData);
    } catch (err: any) {
      console.error('Error loading dashboard:', err);
      const msg = err.response?.data?.error ?? err.message ?? 'Erro ao carregar dashboard';
      setError(msg);
    } finally {
      setIsLoading(false);
    }
  };

  const formatDate = (dateStr: string) => {
    const date = new Date(dateStr);
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    
    const isToday = date.toDateString() === today.toDateString();
    const isTomorrow = date.toDateString() === tomorrow.toDateString();
    const time = date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
    
    if (isToday) return `Hoje, ${time}`;
    if (isTomorrow) return `Amanha, ${time}`;
    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
  };

  if (isLoading) {
    return (
      <Layout>
        <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
          <div className="max-w-7xl mx-auto">
            <div className="mb-8">
              <div className="h-8 w-48 skeleton mb-2"></div>
              <div className="h-4 w-72 skeleton"></div>
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
              {[1, 2, 3, 4].map((i) => (
                <div key={i} className="card p-6">
                  <div className="flex items-center gap-4">
                    <div className="w-12 h-12 skeleton rounded-xl"></div>
                    <div className="flex-1">
                      <div className="h-4 skeleton w-20 mb-2"></div>
                      <div className="h-6 skeleton w-12"></div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </Layout>
    );
  }

  if (error || !stats) {
    return (
      <Layout>
        <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4">
          <div className="max-w-7xl mx-auto">
            <div className="card border-red-100 bg-red-50 p-8 text-center">
              <div className="w-12 h-12 mx-auto bg-red-100 rounded-xl flex items-center justify-center mb-4">
                <svg className="w-6 h-6 text-red-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-red-800">
                {error?.includes('nenhuma organizacao') ? 'Nenhuma organizacao' : 'Erro ao carregar dashboard'}
              </h3>
              <p className="mt-2 text-red-600">{error}</p>
              {error?.includes('nenhuma organizacao') ? (
                <Link href="/notifications" className="btn-primary mt-4 inline-block">
                  Ver notificacoes e convites
                </Link>
              ) : (
                <button onClick={() => loadData(selectedScope === GENERAL_DASHBOARD_VALUE ? undefined : selectedScope)} className="btn-primary mt-4">
                  Tentar novamente
                </button>
              )}
            </div>
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-7xl mx-auto">
          <div className="space-y-8">
              {/* Header + scope selector */}
              <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
                <div>
                  <h1 className="page-title">
                    {selectedScope === GENERAL_DASHBOARD_VALUE && isSiteAdmin
                      ? 'Dashboard geral da plataforma'
                      : 'Dashboard'}
                  </h1>
                  <p className="text-muted mt-1">
                    {selectedScope === GENERAL_DASHBOARD_VALUE && isSiteAdmin
                      ? 'Visao consolidada de reunioes e tarefas de todas as organizacoes.'
                      : `Organizacao: ${
                          organizations.find((o) => o.id === selectedScope)?.name ?? selectedScope
                        } (apenas dados desta organizacao)`}
                  </p>
                </div>
                {(isSiteAdmin && organizations.length > 0) || (!isSiteAdmin && organizations.length > 1) ? (
                  <div className="flex items-center gap-2">
                    <label htmlFor="dashboard-scope" className="text-sm font-medium text-gray-600 whitespace-nowrap">
                      Exibir:
                    </label>
                    <select
                      id="dashboard-scope"
                      value={selectedScope}
                      onChange={(e) => setSelectedScope(e.target.value)}
                      className="rounded-xl border border-gray-200 dark:border-slate-600 bg-white dark:bg-slate-800 px-4 py-2 text-sm text-gray-900 dark:text-slate-100 shadow-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                    >
                      {isSiteAdmin && (
                        <option value={GENERAL_DASHBOARD_VALUE}>Dashboard geral</option>
                      )}
                      {organizations.map((org) => (
                        <option key={org.id} value={org.id}>
                          {org.name} {org.role === 'Owner' ? '(dono)' : ''}
                        </option>
                      ))}
                    </select>
                  </div>
                ) : null}
              </div>

              {/* Stats Cards */}
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5">
                <StatCard
                  icon={<CalendarIcon />}
                  iconBg="bg-indigo-100 text-indigo-600"
                  label="Total de Reunioes"
                  value={stats.totalMeetings}
                  subtitle={`${stats.meetingsThisMonth} este mes`}
                />
                <StatCard
                  icon={<CheckIcon />}
                  iconBg="bg-emerald-100 text-emerald-600"
                  label="Concluidas"
                  value={stats.completedMeetings}
                  subtitle={`${stats.totalMeetingHours}h em reunioes`}
                />
                <StatCard
                  icon={<TaskIcon />}
                  iconBg="bg-amber-100 text-amber-600"
                  label="Tarefas Pendentes"
                  value={stats.pendingTasks}
                  subtitle={`${stats.taskCompletionRate}% concluidas`}
                  highlight={stats.pendingTasks > 10}
                />
                <StatCard
                  icon={<ClockIcon />}
                  iconBg="bg-violet-100 text-violet-600"
                  label="Duracao Media"
                  value={stats.averageMeetingDuration}
                  subtitle="por reuniao"
                />
              </div>

              {/* Main Grid */}
              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Upcoming Meetings */}
                <div className="lg:col-span-2 card p-6">
                  <div className="flex items-center justify-between mb-5">
                    <h2 className="section-title">Proximas Reunioes</h2>
                    <Link href="/" className="text-sm text-indigo-600 hover:text-indigo-700 font-medium">
                      Ver todas
                    </Link>
                  </div>
                  {stats.upcomingMeetings.length === 0 ? (
                    <EmptyState icon={<CalendarIcon />} message="Nenhuma reuniao agendada" />
                  ) : (
                    <div className="space-y-3">
                      {stats.upcomingMeetings.map((meeting, i) => (
                        <Link
                          key={meeting.id}
                          href={`/meetings/${meeting.id}`}
                          className="flex items-center justify-between p-4 rounded-xl bg-gray-50 dark:bg-slate-700/50 hover:bg-gray-100 dark:hover:bg-slate-700 transition-all duration-200 group"
                          style={{ animationDelay: `${i * 50}ms` }}
                        >
                          <div className="flex items-center gap-4">
                            <div className="w-10 h-10 rounded-lg bg-indigo-100 flex items-center justify-center text-indigo-600">
                              <CalendarIcon />
                            </div>
                            <div>
                              <p className="font-medium text-gray-900 group-hover:text-indigo-600 transition-colors">{meeting.title}</p>
                              <p className="text-sm text-gray-500">{formatDate(meeting.scheduledAt)}</p>
                            </div>
                          </div>
                          <div className="flex items-center gap-3">
                            <span className="badge-primary">{typeLabels[meeting.type] || meeting.type}</span>
                            <span className="text-sm text-gray-400">{meeting.participantCount} part.</span>
                          </div>
                        </Link>
                      ))}
                    </div>
                  )}
                </div>

                {/* Tasks by Priority */}
                <div className="card p-6">
                  <div className="flex items-center justify-between mb-5">
                    <h2 className="section-title">Tarefas Pendentes</h2>
                    <Link href="/tasks" className="text-sm text-indigo-600 hover:text-indigo-700 font-medium">
                      Ver todas
                    </Link>
                  </div>
                  {stats.tasksByPriority.length === 0 ? (
                    <EmptyState icon={<TaskIcon />} message="Sem tarefas pendentes" />
                  ) : (
                    <div className="space-y-4">
                      {stats.tasksByPriority.map((item) => {
                        const config = priorityConfig[item.priority] || { label: item.priority, color: 'bg-gray-100 text-gray-700' };
                        return (
                          <div key={item.priority} className="flex items-center justify-between">
                            <span className={`badge ${config.color}`}>{config.label}</span>
                            <div className="flex items-center gap-3">
                              <div className="w-24 bg-gray-100 dark:bg-slate-700 rounded-full h-1.5 overflow-hidden">
                                <div
                                  className="h-full bg-indigo-500 rounded-full transition-all duration-500"
                                  style={{ width: `${Math.min((item.count / stats.totalTasks) * 100, 100)}%` }}
                                />
                              </div>
                              <span className="text-sm font-semibold text-gray-900 w-6 text-right">{item.count}</span>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>

                {/* Recent Meetings */}
                <div className="card p-6">
                  <h2 className="section-title mb-5">Reunioes Recentes</h2>
                  {stats.recentMeetings.length === 0 ? (
                    <EmptyState icon={<CheckIcon />} message="Nenhuma reuniao concluida" />
                  ) : (
                    <div className="space-y-3">
                      {stats.recentMeetings.map((meeting) => (
                        <Link
                          key={meeting.id}
                          href={`/meetings/${meeting.id}`}
                          className="block p-3 rounded-lg hover:bg-gray-50 transition-colors"
                        >
                          <p className="font-medium text-gray-900 text-sm truncate">{meeting.title}</p>
                          <div className="flex items-center gap-3 mt-1 text-xs text-gray-500">
                            <span>{meeting.duration}</span>
                            <span className="w-1 h-1 bg-gray-300 rounded-full"></span>
                            <span>{meeting.decisionCount} decisoes</span>
                            <span className="w-1 h-1 bg-gray-300 rounded-full"></span>
                            <span>{meeting.taskCount} tarefas</span>
                          </div>
                        </Link>
                      ))}
                    </div>
                  )}
                </div>

                {/* Meetings by Type */}
                <div className="lg:col-span-2 card p-6">
                  <h2 className="section-title mb-5">Distribuicao por Tipo</h2>
                  {stats.meetingsByType.length === 0 ? (
                    <EmptyState icon={<ChartIcon />} message="Sem dados disponiveis" />
                  ) : (
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-4">
                      {stats.meetingsByType.map((item) => (
                        <div key={item.type} className="p-4 rounded-xl bg-gray-50">
                          <p className="text-2xl font-bold text-gray-900">{item.count}</p>
                          <p className="text-sm text-gray-500">{typeLabels[item.type] || item.type}</p>
                          <div className="mt-2 w-full bg-gray-200 rounded-full h-1">
                            <div
                              className="h-full bg-indigo-500 rounded-full"
                              style={{ width: `${(item.count / stats.totalMeetings) * 100}%` }}
                            />
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </div>

              {/* Activity Chart */}
              {productivity && productivity.dailyStats.length > 0 && (
                <div className="card p-6">
                  <h2 className="section-title mb-5">Atividade - {productivity.period}</h2>
                  <div className="flex items-end gap-1 h-24">
                    {productivity.dailyStats.map((day, index) => {
                      const maxCount = Math.max(...productivity.dailyStats.map(d => d.meetingCount), 1);
                      const height = Math.max((day.meetingCount / maxCount) * 100, 4);
                      return (
                        <div
                          key={index}
                          className="flex-1 bg-gradient-to-t from-indigo-500 to-indigo-400 rounded-t hover:from-indigo-600 hover:to-indigo-500 transition-colors cursor-pointer"
                          style={{ height: `${height}%` }}
                          title={`${day.date}: ${day.meetingCount} reunioes`}
                        />
                      );
                    })}
                  </div>
                  <div className="flex justify-between mt-2 text-xs text-gray-400">
                    <span>{productivity.dailyStats[0]?.date}</span>
                    <span>{productivity.dailyStats[productivity.dailyStats.length - 1]?.date}</span>
                  </div>
                </div>
              )}
          </div>
        </div>
      </div>
    </Layout>
  );
}

function StatCard({ icon, iconBg, label, value, subtitle, highlight }: {
  icon: React.ReactNode;
  iconBg: string;
  label: string;
  value: number | string;
  subtitle: string;
  highlight?: boolean;
}) {
  return (
    <div className={`card p-5 ${highlight ? 'border-amber-200 bg-amber-50/50' : ''}`}>
      <div className="flex items-center gap-4">
        <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${iconBg}`}>
          {icon}
        </div>
        <div>
          <p className="text-sm text-gray-500">{label}</p>
          <p className="text-2xl font-bold text-gray-900">{value}</p>
        </div>
      </div>
      <p className="mt-3 text-sm text-gray-500">{subtitle}</p>
    </div>
  );
}

function EmptyState({ icon, message }: { icon: React.ReactNode; message: string }) {
  return (
    <div className="text-center py-8">
      <div className="w-12 h-12 mx-auto bg-gray-100 dark:bg-slate-700 rounded-xl flex items-center justify-center text-gray-400 dark:text-slate-400 mb-3">
        {icon}
      </div>
      <p className="text-sm text-gray-500">{message}</p>
    </div>
  );
}

// Icons
function CalendarIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" /></svg>;
}
function CheckIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>;
}
function TaskIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" /></svg>;
}
function ClockIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>;
}
function ChartIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" /></svg>;
}
