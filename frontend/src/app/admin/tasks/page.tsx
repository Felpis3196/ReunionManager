'use client';

import React, { useEffect, useState } from 'react';
import Layout from '@/components/Layout/Layout';
import { useRouter } from 'next/navigation';
import { adminDashboardService, AdminTaskSummary } from '@/services/api';
import { useAuthStore } from '@/stores/authStore';

export default function AdminTasksDashboardPage() {
  const router = useRouter();
  const user = useAuthStore((s) => s.user);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [items, setItems] = useState<AdminTaskSummary[]>([]);
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
        const data = await adminDashboardService.getTasksSummary();
        setItems(data);
      } catch (err: any) {
        console.error('Error loading admin tasks summary:', err);
        const msg =
          err?.response?.data?.error ??
          err?.message ??
          'Erro ao carregar resumo de tarefas por organizacao';
        setError(msg);
      } finally {
        setIsLoading(false);
      }
    })();
  }, [user?.isSiteAdmin]);

  const handleOpenOrgTasks = (orgId: string) => {
    router.push(`/dashboard?organizationId=${orgId}`);
  };

  const totalTasksOverall = items.reduce((acc, x) => acc + x.totalTasks, 0);
  const completedOverall = items.reduce((acc, x) => acc + x.completedTasks, 0);
  const completionOverall =
    totalTasksOverall > 0 ? ((completedOverall / totalTasksOverall) * 100).toFixed(1) : '0.0';

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-7xl mx-auto">
          <div className="mb-8 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="page-title">Admin - Dashboard de tarefas</h1>
              <p className="text-muted mt-1">
                Visao geral de tarefas por organizacao. Use esta pagina para acompanhar a
                produtividade e identificar gargalos.
              </p>
            </div>
          </div>

          {/* Overall summary */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
            <div className="card p-4">
              <p className="text-xs font-medium text-gray-500 uppercase">Total de tarefas</p>
              <p className="mt-1 text-2xl font-semibold text-gray-900">{totalTasksOverall}</p>
            </div>
            <div className="card p-4">
              <p className="text-xs font-medium text-gray-500 uppercase">Concluidas</p>
              <p className="mt-1 text-2xl font-semibold text-emerald-700">{completedOverall}</p>
            </div>
            <div className="card p-4">
              <p className="text-xs font-medium text-gray-500 uppercase">% concluidas</p>
              <p className="mt-1 text-2xl font-semibold text-indigo-700">
                {completionOverall}
                <span className="text-base font-normal text-gray-500 ml-1">%</span>
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
                  <div className="h-5 w-36 bg-gray-200 rounded mb-3" />
                  <div className="space-y-2">
                    <div className="h-3 w-20 bg-gray-200 rounded" />
                    <div className="h-3 w-24 bg-gray-200 rounded" />
                    <div className="h-3 w-28 bg-gray-200 rounded" />
                  </div>
                </div>
              ))}
            </div>
          ) : items.length === 0 ? (
            <div className="card p-10 text-center">
              <h2 className="text-lg font-semibold text-gray-900">
                Nenhuma tarefa encontrada nas organizacoes
              </h2>
              <p className="mt-2 text-sm text-gray-500">
                Quando as reunioes gerarem tarefas, elas aparecerao aqui agregadas por organizacao.
              </p>
            </div>
          ) : (
            <div className="card p-5">
              <div className="overflow-x-auto">
                <table className="min-w-full text-sm">
                  <thead>
                    <tr className="text-left text-xs font-medium text-gray-500 uppercase tracking-wide border-b border-gray-100">
                      <th className="py-2 pr-4">Organizacao</th>
                      <th className="py-2 px-4 text-right">Tarefas</th>
                      <th className="py-2 px-4 text-right">Concluidas</th>
                      <th className="py-2 px-4 text-right">% concluidas</th>
                      <th className="py-2 px-4 text-right">Acoes</th>
                    </tr>
                  </thead>
                  <tbody>
                    {items.map((item) => (
                      <tr key={item.organizationId} className="border-b border-gray-50 last:border-0">
                        <td className="py-3 pr-4">
                          <span className="font-medium text-gray-900">{item.organizationName}</span>
                        </td>
                        <td className="py-3 px-4 text-right text-gray-900">{item.totalTasks}</td>
                        <td className="py-3 px-4 text-right text-emerald-700">
                          {item.completedTasks}
                        </td>
                        <td className="py-3 px-4 text-right">
                          <span className="font-medium text-indigo-700">
                            {item.taskCompletionRate.toFixed(1)}%
                          </span>
                        </td>
                        <td className="py-3 px-4 text-right">
                          <button
                            type="button"
                            onClick={() => handleOpenOrgTasks(item.organizationId)}
                            className="text-xs font-medium text-indigo-600 hover:text-indigo-700"
                          >
                            Ver dashboard da org &rarr;
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}

