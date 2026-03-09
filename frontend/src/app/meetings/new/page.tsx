'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/components/Layout/Layout';
import MeetingForm from '@/components/Meeting/MeetingForm';
import Link from 'next/link';
import { CreateMeetingDto } from '@/types/meeting';
import { meetingService } from '@/services/api';

export default function NewMeetingPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [showSuccess, setShowSuccess] = useState(false);

  const handleSubmit = async (data: CreateMeetingDto) => {
    setIsLoading(true);
    try {
      const meeting = await meetingService.create(data);
      setShowSuccess(true);
      setTimeout(() => {
        router.push(`/meetings/${meeting.id}`);
      }, 1500);
    } catch (error) {
      console.error('Error creating meeting:', error);
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-3xl mx-auto">
          {/* Back Link */}
          <Link
            href="/"
            className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700 mb-6 group"
          >
            <svg className="w-4 h-4 mr-1.5 transition-transform group-hover:-translate-x-1" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
            Voltar para reunioes
          </Link>

          {/* Header */}
          <div className="mb-8">
            <div className="flex items-center gap-4">
              <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-indigo-500 to-violet-500 flex items-center justify-center shadow-lg shadow-indigo-500/25">
                <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
                </svg>
              </div>
              <div>
                <h1 className="page-title">Nova Reuniao</h1>
                <p className="text-muted mt-1">Crie uma nova reuniao e configure os detalhes</p>
              </div>
            </div>
          </div>

          {/* Success Toast */}
          {showSuccess && (
            <div className="fixed top-20 right-4 z-50 animate-slideIn">
              <div className="card border-emerald-100 bg-emerald-50 p-4 flex items-center gap-3 shadow-lg">
                <div className="w-9 h-9 rounded-lg bg-emerald-100 flex items-center justify-center">
                  <svg className="w-5 h-5 text-emerald-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                </div>
                <div>
                  <p className="text-sm font-medium text-emerald-800">Reuniao criada com sucesso!</p>
                  <p className="text-xs text-emerald-600">Redirecionando...</p>
                </div>
              </div>
            </div>
          )}

          {/* Form Card */}
          <div className="card p-6 md:p-8 animate-fadeIn">
            <MeetingForm onSubmit={handleSubmit} isLoading={isLoading} />
          </div>

          {/* Tips */}
          <div className="mt-6 card border-indigo-50 bg-indigo-50/50 p-5">
            <div className="flex gap-4">
              <div className="flex-shrink-0 w-9 h-9 rounded-lg bg-indigo-100 flex items-center justify-center">
                <svg className="w-5 h-5 text-indigo-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
                </svg>
              </div>
              <div>
                <h3 className="text-sm font-medium text-indigo-900">Dicas para uma reuniao produtiva</h3>
                <ul className="mt-2 space-y-1.5 text-sm text-indigo-700">
                  <li className="flex items-start gap-2">
                    <span className="w-1.5 h-1.5 rounded-full bg-indigo-400 mt-1.5 flex-shrink-0"></span>
                    Use titulos claros e descritivos
                  </li>
                  <li className="flex items-start gap-2">
                    <span className="w-1.5 h-1.5 rounded-full bg-indigo-400 mt-1.5 flex-shrink-0"></span>
                    Defina objetivos na descricao
                  </li>
                  <li className="flex items-start gap-2">
                    <span className="w-1.5 h-1.5 rounded-full bg-indigo-400 mt-1.5 flex-shrink-0"></span>
                    Ajuste a duracao conforme os topicos
                  </li>
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Layout>
  );
}
