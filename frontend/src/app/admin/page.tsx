'use client';

import React, { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/components/Layout/Layout';
import { adminDashboardService, AdminOrganizationSummary } from '@/services/api';
import { useAuthStore } from '@/stores/authStore';

export default function AdminOverviewPage() {
  const router = useRouter();
  const user = useAuthStore((s) => s.user);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [items, setItems] = useState<AdminOrganizationSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isAuthenticated === false) {
      router.replace('/login');
      return;
    }
    if (user && !user.isSiteAdmin) {
      router.replace('/dashboard');
    }
  }, [user, isAuthenticated, router]);

  useEffect(() => {
    if (!user?.isSiteAdmin) return;
    (async () => {
      try {
        setIsLoading(true);
        setError(null);
        const data = await adminDashboardService.getOrganizationsSummary();
        setItems(data);
      } catch (err: any) {
        console.error('Error loading admin organizations summary:', err);
        const msg =
          err?.response?.data?.error ??
          err?.message ??
          'Erro ao carregar resumo das organizacoes';
        setError(msg);
      } finally {
        setIsLoading(false);
      }
    })();
  }, [user?.isSiteAdmin]);

  const handleOpenOrgDashboard = (orgId: string) => {
    router.push(`/dashboard?organizationId=${orgId}`);
  };

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-7xl mx-auto">
          <div className="mb-8 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="page-title">Admin - Visao geral por organizacao</h1>
              <p className="text-muted mt-1">
                Este painel mostra uma visao consolidada das organizacoes da plataforma. Apenas
                administradores do site veem estes dados.
              </p>
            </div>
          </div>

          {error && (
            <div className="card border-red-100 bg-red-50 p-4 mb-6">
              <p className="text-sm text-red-700">{error}</p>
            </div>
          )}

          {isLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
              {[1, 2, 3].map((i) => (
                <div key={i} className="card p-5 animate-pulse">
                  <div className="h-5 w-40 bg-gray-200 rounded mb-3" />
                  <div className="space-y-2">
                    <div className="h-3 w-24 bg-gray-200 rounded" />
                    <div className="h-3 w-32 bg-gray-200 rounded" />
                    <div className="h-3 w-28 bg-gray-200 rounded" />
                  </div>
                </div>
              ))}
            </div>
          ) : items.length === 0 ? (
            <div className="card p-10 text-center">
              <h2 className="text-lg font-semibold text-gray-900">
                Nenhuma organizacao encontrada
              </h2>
              <p className="mt-2 text-sm text-gray-500">
                Crie ou convide organizacoes para ver os dados agregados aqui.
              </p>
            </div>
          ) : (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
              {items.map((org) => (
                <button
                  key={org.organizationId}
                  type="button"
                  onClick={() => handleOpenOrgDashboard(org.organizationId)}
                  className="card p-5 text-left hover:border-indigo-200 hover:shadow-md transition-all"
                >
                  <div className="flex items-center justify-between mb-3">
                    <h2 className="text-base font-semibold text-gray-900">{org.name}</h2>
                    <span className="text-xs font-medium px-2.5 py-1 rounded-full bg-indigo-50 text-indigo-700">
                      {org.totalUsers} membros
                    </span>
                  </div>
                  <dl className="space-y-1.5 text-sm">
                    <div className="flex justify-between">
                      <dt className="text-gray-500">Reunioes</dt>
                      <dd className="font-medium text-gray-900">{org.totalMeetings}</dd>
                    </div>
                    <div className="flex justify-between">
                      <dt className="text-gray-500">Tarefas</dt>
                      <dd className="font-medium text-gray-900">
                        {org.totalTasks} ({org.completedTasks} concluidas)
                      </dd>
                    </div>
                    <div className="flex justify-between">
                      <dt className="text-gray-500">% Tarefas concluidas</dt>
                      <dd className="font-medium text-emerald-700">
                        {org.taskCompletionRate.toFixed(1)}%
                      </dd>
                    </div>
                  </dl>
                  <div className="mt-4 text-sm text-indigo-600 font-medium">
                    Ver dashboard da organizacao &rarr;
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}

