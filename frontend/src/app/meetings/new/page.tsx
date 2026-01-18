'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/components/Layout/Layout';
import MeetingForm from '@/components/Meeting/MeetingForm';
import { CreateMeetingDto } from '@/types/meeting';
import { meetingService } from '@/services/api';

export default function NewMeetingPage() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (data: CreateMeetingDto) => {
    setIsLoading(true);
    try {
      const meeting = await meetingService.create(data);
      router.push(`/meetings/${meeting.id}`);
    } catch (error) {
      console.error('Error creating meeting:', error);
      alert('Erro ao criar reunião. Tente novamente.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Layout>
      <div className="max-w-3xl mx-auto">
        <h1 className="text-3xl font-bold text-gray-900 mb-8">Nova Reunião</h1>
        <div className="bg-white rounded-lg shadow-sm p-6">
          <MeetingForm onSubmit={handleSubmit} isLoading={isLoading} />
        </div>
      </div>
    </Layout>
  );
}
