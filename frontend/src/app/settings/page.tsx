'use client';

import React, { useEffect, useState } from 'react';
import Layout from '@/components/Layout/Layout';
import { useAuthStore } from '@/stores/authStore';
import { useRouter } from 'next/navigation';
import {
  getTheme,
  setTheme,
  getEmailReminders,
  setEmailReminders,
  getNotifyNewTasks,
  setNotifyNewTasks,
  getDateFormat,
  setDateFormat,
  type ThemeValue,
  type DateFormatValue,
} from '@/lib/theme';

export default function SettingsPage() {
  const router = useRouter();
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  const [theme, setThemeState] = useState<ThemeValue>('system');
  const [emailReminders, setEmailRemindersState] = useState(true);
  const [notifyNewTasks, setNotifyNewTasksState] = useState(true);
  const [dateFormat, setDateFormatState] = useState<DateFormatValue>('pt-BR');

  useEffect(() => {
    const t = setTimeout(() => {
      if (isAuthenticated === false) router.replace('/login');
    }, 100);
    return () => clearTimeout(t);
  }, [isAuthenticated, router]);

  useEffect(() => {
    setThemeState(getTheme());
    setEmailRemindersState(getEmailReminders());
    setNotifyNewTasksState(getNotifyNewTasks());
    setDateFormatState(getDateFormat());
  }, []);

  const handleThemeChange = (value: ThemeValue) => {
    setTheme(value);
    setThemeState(value);
  };

  const handleEmailRemindersChange = (checked: boolean) => {
    setEmailReminders(checked);
    setEmailRemindersState(checked);
  };

  const handleNotifyNewTasksChange = (checked: boolean) => {
    setNotifyNewTasks(checked);
    setNotifyNewTasksState(checked);
  };

  const handleDateFormatChange = (value: DateFormatValue) => {
    setDateFormat(value);
    setDateFormatState(value);
  };

  if (isAuthenticated === false) return null;

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-2xl mx-auto">
          <div className="mb-8">
            <h1 className="page-title">Configurações</h1>
            <p className="text-muted mt-1">
              Preferências de aparência, notificações e formato
            </p>
          </div>

          {/* Aparência */}
          <section className="card p-6 mb-6">
            <h2 className="section-title mb-1">Aparência</h2>
            <p className="text-muted mb-4">
              Escolha o tema de exibição da aplicação.
            </p>
            <div className="space-y-2">
              <label className="input-label">Tema</label>
              <select
                value={theme}
                onChange={(e) => handleThemeChange(e.target.value as ThemeValue)}
                className="rounded-xl border border-gray-200 dark:border-slate-600 bg-white dark:bg-slate-800 px-4 py-2.5 text-sm text-gray-900 dark:text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none"
              >
                <option value="light">Claro</option>
                <option value="dark">Escuro</option>
                <option value="system">Seguir sistema</option>
              </select>
              <p className="text-xs text-gray-500 dark:text-slate-400 mt-1">
                &quot;Seguir sistema&quot; usa a preferência do seu dispositivo.
              </p>
            </div>
          </section>

          {/* Notificações */}
          <section className="card p-6 mb-6">
            <h2 className="section-title mb-1">Notificações</h2>
            <p className="text-muted mb-4">
              Defina como deseja ser avisado sobre reuniões e tarefas.
            </p>
            <div className="space-y-4">
              <label className="flex items-center justify-between gap-4 cursor-pointer">
                <div>
                  <span className="block text-sm font-medium text-gray-900 dark:text-slate-100">
                    Lembretes de reunião por e-mail
                  </span>
                  <span className="block text-xs text-gray-500 dark:text-slate-400 mt-0.5">
                    Receber e-mails de lembrete antes das reuniões agendadas.
                  </span>
                </div>
                <input
                  type="checkbox"
                  checked={emailReminders}
                  onChange={(e) => handleEmailRemindersChange(e.target.checked)}
                  className="w-5 h-5 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                />
              </label>
              <label className="flex items-center justify-between gap-4 cursor-pointer">
                <div>
                  <span className="block text-sm font-medium text-gray-900 dark:text-slate-100">
                    Novas tarefas atribuídas
                  </span>
                  <span className="block text-xs text-gray-500 dark:text-slate-400 mt-0.5">
                    Ser notificado quando uma nova tarefa for atribuída a você.
                  </span>
                </div>
                <input
                  type="checkbox"
                  checked={notifyNewTasks}
                  onChange={(e) => handleNotifyNewTasksChange(e.target.checked)}
                  className="w-5 h-5 rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
                />
              </label>
            </div>
          </section>

          {/* Formato */}
          <section className="card p-6">
            <h2 className="section-title mb-1">Formato</h2>
            <p className="text-muted mb-4">
              Preferência de exibição de datas na aplicação.
            </p>
            <div className="space-y-2">
              <label className="input-label">Formato de data</label>
              <select
                value={dateFormat}
                onChange={(e) =>
                  handleDateFormatChange(e.target.value as DateFormatValue)
                }
                className="rounded-xl border border-gray-200 dark:border-slate-600 bg-white dark:bg-slate-800 px-4 py-2.5 text-sm text-gray-900 dark:text-slate-100 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 focus:outline-none"
              >
                <option value="pt-BR">DD/MM/AAAA (pt-BR)</option>
                <option value="en-US">MM/DD/AAAA (en-US)</option>
              </select>
              <p className="text-xs text-gray-500 dark:text-slate-400 mt-1">
                Será aplicado na exibição de datas em listas e relatórios.
              </p>
            </div>
          </section>
        </div>
      </div>
    </Layout>
  );
}
