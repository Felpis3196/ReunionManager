'use client';

import React from 'react';
import Layout from '@/components/Layout/Layout';
import MeetingCard from '@/components/Meeting/MeetingCard';
import Link from 'next/link';
import { Meeting } from '@/types/meeting';

// Mock data - in real app, fetch from API
const mockMeetings: Meeting[] = [];

export default function HomePage() {
  return (
    <Layout>
      <div className="mb-8 flex justify-between items-center">
        <h1 className="text-3xl font-bold text-gray-900">Reuniões</h1>
        <Link
          href="/meetings/new"
          className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 transition-colors"
        >
          Nova Reunião
        </Link>
      </div>

      {mockMeetings.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 mb-4">Nenhuma reunião encontrada.</p>
          <Link
            href="/meetings/new"
            className="text-primary-600 hover:text-primary-700 font-medium"
          >
            Criar primeira reunião
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {mockMeetings.map((meeting) => (
            <MeetingCard key={meeting.id} meeting={meeting} />
          ))}
        </div>
      )}
    </Layout>
  );
}
