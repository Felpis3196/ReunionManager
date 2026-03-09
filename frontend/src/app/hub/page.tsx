'use client';

import React from 'react';
import Link from 'next/link';
import Layout from '@/components/Layout/Layout';
import { useAuthStore } from '@/stores/authStore';

const cards = [
  { href: '/', label: 'Reunioes', description: 'Ver e agendar reunioes', icon: CalendarIcon },
  { href: '/dashboard', label: 'Dashboard', description: 'Visao geral e metricas', icon: ChartIcon },
  { href: '/tasks', label: 'Tarefas', description: 'Gerenciar tarefas', icon: TaskIcon },
  { href: '/notifications', label: 'Notificacoes', description: 'Convites e avisos', icon: BellIcon },
];

export default function HubPage() {
  const { user } = useAuthStore();
  const hasOrg = !!user?.organizationId;

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-4xl mx-auto">
          <h1 className="page-title">Central</h1>
          <p className="text-muted mt-1">Acesso rapido as areas do sistema</p>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mt-8">
            {cards.map((item) => {
              const Icon = item.icon;
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className="card p-6 hover:shadow-lg transition-shadow duration-200 flex flex-col gap-3 group"
                >
                  <div className="w-12 h-12 rounded-xl bg-indigo-100 flex items-center justify-center text-indigo-600 group-hover:bg-indigo-600 group-hover:text-white transition-colors">
                    <Icon className="w-6 h-6" />
                  </div>
                  <h2 className="font-semibold text-gray-900">{item.label}</h2>
                  <p className="text-sm text-gray-500 flex-1">{item.description}</p>
                </Link>
              );
            })}

            {hasOrg && (
              <>
                <Link
                  href="/team"
                  className="card p-6 hover:shadow-lg transition-shadow duration-200 flex flex-col gap-3 group"
                >
                  <div className="w-12 h-12 rounded-xl bg-indigo-100 flex items-center justify-center text-indigo-600 group-hover:bg-indigo-600 group-hover:text-white transition-colors">
                    <TeamIcon className="w-6 h-6" />
                  </div>
                  <h2 className="font-semibold text-gray-900">Equipe</h2>
                  <p className="text-sm text-gray-500 flex-1">Membros e convites da organizacao</p>
                </Link>
                <Link
                  href="/chat"
                  className="card p-6 hover:shadow-lg transition-shadow duration-200 flex flex-col gap-3 group"
                >
                  <div className="w-12 h-12 rounded-xl bg-emerald-100 flex items-center justify-center text-emerald-600 group-hover:bg-emerald-600 group-hover:text-white transition-colors">
                    <ChatIcon className="w-6 h-6" />
                  </div>
                  <h2 className="font-semibold text-gray-900">Chat da equipe</h2>
                  <p className="text-sm text-gray-500 flex-1">Conversa em tempo real com a equipe</p>
                </Link>
              </>
            )}
          </div>

          {!hasOrg && (
            <div className="mt-8 p-4 rounded-xl border bg-amber-50 border-amber-200 text-amber-800">
              <p className="text-sm font-medium">Entre em uma organizacao para acessar Equipe e Chat. Aceite um convite em Notificacoes.</p>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}

function CalendarIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
    </svg>
  );
}
function ChartIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
    </svg>
  );
}
function TaskIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
    </svg>
  );
}
function BellIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
    </svg>
  );
}
function TeamIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
    </svg>
  );
}
function ChatIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
    </svg>
  );
}
