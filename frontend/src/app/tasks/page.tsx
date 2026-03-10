'use client';

import React, { useEffect, useState, useCallback } from 'react';
import Layout from '@/components/Layout/Layout';
import Link from 'next/link';
import { taskService, Task } from '@/services/api';
import { useAuthStore } from '@/stores/authStore';

const statusConfig: Record<string, { label: string; color: string; bg: string }> = {
  Pending: { label: 'Pendente', color: 'text-amber-700', bg: 'bg-amber-50' },
  InProgress: { label: 'Em Andamento', color: 'text-blue-700', bg: 'bg-blue-50' },
  Completed: { label: 'Concluida', color: 'text-emerald-700', bg: 'bg-emerald-50' },
  Cancelled: { label: 'Cancelada', color: 'text-gray-500', bg: 'bg-gray-100' },
};

const priorityConfig: Record<string, { label: string; color: string; dot: string }> = {
  Low: { label: 'Baixa', color: 'text-gray-600 dark:text-slate-400', dot: 'bg-gray-400 dark:bg-slate-500' },
  Medium: { label: 'Media', color: 'text-blue-600 dark:text-blue-400', dot: 'bg-blue-500' },
  High: { label: 'Alta', color: 'text-amber-600 dark:text-amber-400', dot: 'bg-amber-500' },
  Critical: { label: 'Critica', color: 'text-red-600 dark:text-red-400', dot: 'bg-red-500' },
};

export default function TasksPage() {
  const user = useAuthStore((s) => s.user);
  const [tasks, setTasks] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<'all' | 'pending' | 'completed'>('pending');
  const [scope, setScope] = useState<'mine' | 'all'>('mine');

  const loadTasks = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const statusFilter = filter === 'pending' ? 'Pending' : filter === 'completed' ? 'Completed' : undefined;
      const assignedToId = scope === 'mine' && user?.id ? user.id : undefined;
      const data = await taskService.getAll({ status: statusFilter, assignedToId });
      setTasks(data);
    } catch (err: any) {
      console.error('Error loading tasks:', err);
      setError(err.message || 'Erro ao carregar tarefas');
    } finally {
      setIsLoading(false);
    }
  }, [filter, scope, user?.id]);

  useEffect(() => {
    loadTasks();
  }, [filter, loadTasks]);

  const handleComplete = async (task: Task) => {
    try {
      await taskService.complete(task.id);
      loadTasks();
    } catch (err: any) {
      console.error('Error completing task:', err);
      alert(err.response?.data?.error || 'Erro ao concluir tarefa');
    }
  };

  const handleDelete = async (task: Task) => {
    if (!confirm('Tem certeza que deseja excluir esta tarefa?')) return;
    try {
      await taskService.delete(task.id);
      loadTasks();
    } catch (err: any) {
      console.error('Error deleting task:', err);
      alert(err.response?.data?.error || 'Erro ao excluir tarefa');
    }
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return null;
    const date = new Date(dateStr);
    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short' });
  };

  const isOverdue = (task: Task) => {
    if (!task.dueDate || task.status === 'Completed') return false;
    return new Date(task.dueDate) < new Date();
  };

  const canComplete = (task: Task) => {
    if (task.status === 'Completed' || task.status === 'Cancelled') return false;
    const userId = user?.id;
    if (!userId) return false;
    return task.assignedToId === userId || user?.canCompleteAnyTask === true || user?.canManageTasks === true;
  };

  const canDelete = user?.canManageTasks === true;

  const filterOptions = [
    { id: 'all', label: 'Todas' },
    { id: 'pending', label: 'Pendentes' },
    { id: 'completed', label: 'Concluidas' },
  ];

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-4xl mx-auto">
          {/* Header */}
          <div className="mb-8">
            <h1 className="page-title">Tarefas</h1>
            <p className="text-muted mt-1">
              {user?.isSiteAdmin
                ? 'Tarefas da organizacao atual. Para visao agregada de todas as organizacoes, use o dashboard de admin.'
                : 'Gerencie as tarefas da sua organizacao atual, separando entre suas tarefas e as da equipe.'}
            </p>
          </div>

          {/* Filters */}
          <div className="mb-6 flex flex-wrap items-center gap-2">
            <div className="flex items-center bg-gray-100 dark:bg-slate-700 rounded-lg p-1">
              {filterOptions.map((opt) => (
                <button
                  key={opt.id}
                  onClick={() => setFilter(opt.id as typeof filter)}
                  className={`px-4 py-2 rounded-md text-sm font-medium transition-all duration-200 ${
                    filter === opt.id
                      ? 'bg-white dark:bg-slate-600 text-gray-900 dark:text-white shadow-sm'
                      : 'text-gray-600 dark:text-slate-300 hover:text-gray-900 dark:hover:text-white'
                  }`}
                >
                  {opt.label}
                </button>
              ))}
            </div>
            {user?.canViewAllTasks && (
              <div className="flex items-center bg-gray-100 dark:bg-slate-700 rounded-lg p-1">
                <button
                  onClick={() => setScope('mine')}
                  className={`px-3 py-2 rounded-md text-sm font-medium transition-all ${
                    scope === 'mine' ? 'bg-white dark:bg-slate-600 text-gray-900 dark:text-white shadow-sm' : 'text-gray-600 dark:text-slate-300 hover:text-gray-900 dark:hover:text-white'
                  }`}
                >
                  Minhas tarefas
                </button>
                <button
                  onClick={() => setScope('all')}
                  className={`px-3 py-2 rounded-md text-sm font-medium transition-all ${
                    scope === 'all' ? 'bg-white dark:bg-slate-600 text-gray-900 dark:text-white shadow-sm' : 'text-gray-600 dark:text-slate-300 hover:text-gray-900 dark:hover:text-white'
                  }`}
                >
                  Todas da equipe
                </button>
              </div>
            )}
            <span className="text-sm text-gray-400 dark:text-slate-400 ml-auto">{tasks.length} tarefas</span>
          </div>

          {/* Error */}
          {error && (
            <div className="mb-6 card border-red-100 dark:border-red-900/50 bg-red-50 dark:bg-red-900/20 p-4 animate-fadeIn">
              <div className="flex items-center gap-3">
                <div className="w-9 h-9 rounded-lg bg-red-100 dark:bg-red-900/50 flex items-center justify-center">
                  <svg className="w-5 h-5 text-red-600 dark:text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                  </svg>
                </div>
                <div className="flex-1">
                  <p className="text-sm text-red-800 dark:text-red-200">{error}</p>
                </div>
                <button onClick={loadTasks} className="text-sm font-medium text-red-700 dark:text-red-300 hover:text-red-800 dark:hover:text-red-200">
                  Tentar novamente
                </button>
              </div>
            </div>
          )}

          {/* Loading */}
          {isLoading && (
            <div className="card divide-y divide-gray-100 dark:divide-slate-700">
              {[1, 2, 3, 4, 5].map((i) => (
                <div key={i} className="p-4 flex items-center gap-4">
                  <div className="w-5 h-5 skeleton rounded"></div>
                  <div className="flex-1">
                    <div className="h-4 skeleton w-1/2 mb-2"></div>
                    <div className="h-3 skeleton w-1/4"></div>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Empty State */}
          {!isLoading && !error && tasks.length === 0 && (
            <div className="card p-12 text-center animate-fadeIn">
              <div className="w-16 h-16 mx-auto bg-gray-100 dark:bg-slate-700 rounded-2xl flex items-center justify-center mb-4">
                <svg className="w-8 h-8 text-gray-400 dark:text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
                </svg>
              </div>
              <h3 className="text-lg font-semibold text-gray-900 dark:text-slate-100">
                {filter === 'completed' ? 'Nenhuma tarefa concluida' : 'Nenhuma tarefa pendente'}
              </h3>
              <p className="mt-2 text-sm text-gray-500 dark:text-slate-400">As tarefas sao criadas a partir das reunioes</p>
              <Link href="/" className="btn-primary mt-6 inline-flex">
                Ver reunioes
              </Link>
            </div>
          )}

          {/* Tasks List */}
          {!isLoading && !error && tasks.length > 0 && (
            <div className="card divide-y divide-gray-100 dark:divide-slate-700 animate-fadeIn">
              {tasks.map((task, index) => {
                const status = statusConfig[task.status] || statusConfig.Pending;
                const priority = priorityConfig[task.priority] || priorityConfig.Medium;
                const overdue = isOverdue(task);

                return (
                  <div
                    key={task.id}
                    className={`p-4 hover:bg-gray-50 dark:hover:bg-slate-700/50 transition-colors ${overdue ? 'bg-red-50/50 dark:bg-red-900/20' : ''}`}
                    style={{ animationDelay: `${index * 30}ms` }}
                  >
                    <div className="flex items-start gap-4 group">
                      {/* Checkbox - only enabled if user can complete this task */}
                      <button
                        onClick={() => canComplete(task) && handleComplete(task)}
                        disabled={!canComplete(task) || task.status === 'Completed'}
                        className={`mt-0.5 w-5 h-5 rounded border-2 flex items-center justify-center flex-shrink-0 transition-all duration-200 ${
                          task.status === 'Completed'
                            ? 'bg-emerald-500 border-emerald-500'
                            : canComplete(task)
                              ? 'border-gray-300 dark:border-slate-500 hover:border-emerald-500 hover:bg-emerald-50 dark:hover:bg-emerald-900/30'
                              : 'border-gray-200 dark:border-slate-600 bg-gray-50 dark:bg-slate-700 cursor-not-allowed opacity-60'
                        }`}
                      >
                        {task.status === 'Completed' && (
                          <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" />
                          </svg>
                        )}
                      </button>

                      {/* Content */}
                      <div className="flex-1 min-w-0">
                        <div className="flex items-start justify-between gap-4">
                          <div>
                            <h3 className={`font-medium ${task.status === 'Completed' ? 'text-gray-400 dark:text-slate-500 line-through' : 'text-gray-900 dark:text-slate-100'}`}>
                              {task.title}
                            </h3>
                            {task.description && (
                              <p className="text-sm text-gray-500 dark:text-slate-400 mt-0.5 line-clamp-1">{task.description}</p>
                            )}
                          </div>

                          {/* Delete Button - only for users with ManageTasks */}
                          {canDelete && (
                            <button
                              onClick={() => handleDelete(task)}
                              className="btn-icon-danger opacity-0 group-hover:opacity-100 transition-opacity"
                            >
                              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                              </svg>
                            </button>
                          )}
                        </div>

                        {/* Meta */}
                        <div className="flex flex-wrap items-center gap-3 mt-2">
                          <span className="flex items-center gap-1.5 text-xs">
                            <span className={`w-1.5 h-1.5 rounded-full ${priority.dot}`}></span>
                            <span className={priority.color}>{priority.label}</span>
                          </span>

                          {task.dueDate && (
                            <span className={`text-xs flex items-center gap-1 ${overdue ? 'text-red-600 dark:text-red-400 font-medium' : 'text-gray-500 dark:text-slate-400'}`}>
                              <svg className="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
                              </svg>
                              {formatDate(task.dueDate)}
                              {overdue && ' (Atrasada)'}
                            </span>
                          )}

                          <Link
                            href={`/meetings/${task.meetingId}`}
                            className="text-xs text-indigo-600 dark:text-indigo-400 hover:text-indigo-700 dark:hover:text-indigo-300"
                          >
                            Ver reuniao
                          </Link>
                        </div>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}
