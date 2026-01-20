import React from 'react';
import Link from 'next/link';
import { Meeting, MeetingStatus } from '@/types/meeting';
import { formatDate, formatDuration } from '@/lib/utils';
import { cn } from '@/lib/utils';

interface MeetingCardProps {
  meeting: Meeting;
}

export default function MeetingCard({ meeting }: MeetingCardProps) {
  const statusColors: Record<MeetingStatus, string> = {
    [MeetingStatus.Scheduled]: 'bg-blue-100 text-blue-800',
    [MeetingStatus.InProgress]: 'bg-green-100 text-green-800',
    [MeetingStatus.Completed]: 'bg-gray-100 text-gray-800',
    [MeetingStatus.Cancelled]: 'bg-red-100 text-red-800',
  };

  return (
    <Link href={`/meetings/${meeting.id}`}>
      <div className="bg-white rounded-lg shadow-sm p-6 hover:shadow-md transition-shadow cursor-pointer">
        <div className="flex justify-between items-start mb-4">
          <h3 className="text-lg font-semibold text-gray-900">{meeting.title}</h3>
          <span className={cn('px-2 py-1 text-xs font-medium rounded-full', statusColors[meeting.status])}>
            {meeting.status}
          </span>
        </div>
        
        <p className="text-sm text-gray-600 mb-4 line-clamp-2">{meeting.description}</p>
        
        <div className="flex items-center justify-between text-sm text-gray-500">
          <div className="flex items-center space-x-4">
            <span>{formatDate(meeting.scheduledAt)}</span>
            <span>{formatDuration(meeting.duration)}</span>
          </div>
          <span className="text-gray-400">{meeting.participants.length} participantes</span>
        </div>
      </div>
    </Link>
  );
}
