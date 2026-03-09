'use client';

import React, { useEffect, useState } from 'react';
import Layout from '@/components/Layout/Layout';
import MeetingCard from '@/components/Meeting/MeetingCard';
import Link from 'next/link';
import { Meeting } from '@/types/meeting';
import { meetingService } from '@/services/api';

export default function HomePage() {
  const [meetings, setMeetings] = useState<Meeting[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadMeetings();
  }, []);

  const loadMeetings = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await meetingService.getAll();
      setMeetings(data);
    } catch (err: any) {
      console.error('Error loading meetings:', err);
      setError(err.message || 'Erro ao carregar reunioes');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-7xl mx-auto">
          {/* Header */}
          <div className="mb-8 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
              <h1 className="page-title">Reunioes</h1>
              <p className="mt-1 text-muted">
                Gerencie suas reunioes e acompanhe o progresso
              </p>
            </div>
            <Link href="/meetings/new" className="btn-primary">
              <PlusIcon className="w-5 h-5" />
              Nova Reuniao
            </Link>
          </div>

          {/* Error Message */}
          {error && (
            <div className="mb-6 card border-red-100 bg-red-50 p-4 animate-fadeIn">
              <div className="flex items-start gap-3">
                <div className="flex-shrink-0 w-10 h-10 rounded-full bg-red-100 flex items-center justify-center">
                  <ExclamationIcon className="w-5 h-5 text-red-600" />
                </div>
                <div className="flex-1">
                  <h3 className="text-sm font-medium text-red-800">Erro ao carregar reunioes</h3>
                  <p className="mt-1 text-sm text-red-600">{error}</p>
                  <button
                    onClick={loadMeetings}
                    className="mt-3 text-sm font-medium text-red-700 hover:text-red-800 inline-flex items-center gap-1"
                  >
                    <RefreshIcon className="w-4 h-4" />
                    Tentar novamente
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Loading State */}
          {isLoading && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
              {[1, 2, 3, 4, 5, 6].map((i) => (
                <div key={i} className="card p-5 animate-pulse">
                  <div className="flex items-start justify-between">
                    <div className="h-5 bg-gray-200 rounded w-2/3"></div>
                    <div className="h-5 w-16 bg-gray-200 rounded-full"></div>
                  </div>
                  <div className="mt-4 space-y-2">
                    <div className="h-4 bg-gray-200 rounded w-1/2"></div>
                    <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                  </div>
                  <div className="mt-4 flex gap-2">
                    <div className="h-8 w-8 bg-gray-200 rounded-full"></div>
                    <div className="h-8 w-8 bg-gray-200 rounded-full"></div>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Empty State */}
          {!isLoading && !error && meetings.length === 0 && (
            <div className="card p-12 text-center animate-fadeIn">
              <div className="w-16 h-16 mx-auto bg-gradient-to-br from-indigo-100 to-violet-100 rounded-2xl flex items-center justify-center mb-4">
                <CalendarIcon className="w-8 h-8 text-indigo-600" />
              </div>
              <h3 className="text-lg font-semibold text-gray-900">Nenhuma reuniao encontrada</h3>
              <p className="mt-2 text-sm text-gray-500 max-w-sm mx-auto">
                Comece criando sua primeira reuniao para organizar seus encontros
              </p>
              <Link href="/meetings/new" className="btn-primary mt-6 inline-flex">
                <PlusIcon className="w-5 h-5" />
                Criar primeira reuniao
              </Link>
            </div>
          )}

          {/* Meetings Grid */}
          {!isLoading && !error && meetings.length > 0 && (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
              {meetings.map((meeting, index) => (
                <div
                  key={meeting.id}
                  className="animate-fadeIn"
                  style={{ animationDelay: `${index * 50}ms` }}
                >
                  <MeetingCard meeting={meeting} />
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}

function PlusIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
    </svg>
  );
}

function ExclamationIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
    </svg>
  );
}

function RefreshIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
    </svg>
  );
}

function CalendarIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
    </svg>
  );
}
