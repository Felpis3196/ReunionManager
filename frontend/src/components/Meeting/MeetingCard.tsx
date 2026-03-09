import React from 'react';
import Link from 'next/link';
import { Meeting, MeetingStatus, MeetingType } from '@/types/meeting';

interface MeetingCardProps {
  meeting: Meeting;
}

const statusConfig: Record<MeetingStatus, { label: string; color: string; bg: string; dot: string }> = {
  [MeetingStatus.Scheduled]: { label: 'Agendada', color: 'text-indigo-700', bg: 'bg-indigo-50', dot: 'bg-indigo-500' },
  [MeetingStatus.InProgress]: { label: 'Em Andamento', color: 'text-emerald-700', bg: 'bg-emerald-50', dot: 'bg-emerald-500' },
  [MeetingStatus.Completed]: { label: 'Concluida', color: 'text-gray-600', bg: 'bg-gray-100', dot: 'bg-gray-400' },
  [MeetingStatus.Cancelled]: { label: 'Cancelada', color: 'text-red-700', bg: 'bg-red-50', dot: 'bg-red-500' },
};

const typeIcons: Record<MeetingType, React.ReactNode> = {
  [MeetingType.Planning]: <PlanningIcon />,
  [MeetingType.Review]: <ReviewIcon />,
  [MeetingType.Standup]: <StandupIcon />,
  [MeetingType.Retrospective]: <RetroIcon />,
  [MeetingType.OneOnOne]: <OneOnOneIcon />,
  [MeetingType.Other]: <OtherIcon />,
};

export default function MeetingCard({ meeting }: MeetingCardProps) {
  const status = statusConfig[meeting.status];
  const icon = typeIcons[meeting.type] || typeIcons[MeetingType.Other];

  const formatDateTime = (dateStr: string) => {
    const date = new Date(dateStr);
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    const isToday = date.toDateString() === today.toDateString();
    const isTomorrow = date.toDateString() === tomorrow.toDateString();

    const time = date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
    
    if (isToday) return `Hoje, ${time}`;
    if (isTomorrow) return `Amanha, ${time}`;
    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' });
  };

  const formatDuration = (duration: string) => {
    if (!duration) return '';
    const parts = duration.split(':');
    if (parts.length >= 2) {
      const hours = parseInt(parts[0]);
      const minutes = parseInt(parts[1]);
      if (hours > 0 && minutes > 0) return `${hours}h ${minutes}m`;
      if (hours > 0) return `${hours}h`;
      return `${minutes}m`;
    }
    return duration;
  };

  return (
    <Link href={`/meetings/${meeting.id}`} className="block group">
      <div className="card-hover p-5 h-full flex flex-col">
        {/* Header */}
        <div className="flex items-start justify-between gap-3 mb-3">
          <div className="flex items-center gap-3 flex-1 min-w-0">
            <div className={`w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 ${
              meeting.status === MeetingStatus.Completed ? 'bg-gray-100 text-gray-500' : 
              meeting.status === MeetingStatus.InProgress ? 'bg-emerald-100 text-emerald-600' :
              'bg-indigo-100 text-indigo-600'
            }`}>
              {icon}
            </div>
            <h3 className="font-semibold text-gray-900 truncate group-hover:text-indigo-600 transition-colors">
              {meeting.title}
            </h3>
          </div>
          <span className={`flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-medium ${status.bg} ${status.color}`}>
            <span className={`w-1.5 h-1.5 rounded-full ${status.dot}`}></span>
            {status.label}
          </span>
        </div>

        {/* Description */}
        {meeting.description && (
          <p className="text-sm text-gray-500 line-clamp-2 mb-4 flex-1">
            {meeting.description}
          </p>
        )}
        {!meeting.description && <div className="flex-1" />}

        {/* Footer */}
        <div className="flex items-center justify-between pt-3 border-t border-gray-100">
          <div className="flex items-center gap-3 text-sm text-gray-500">
            <span className="flex items-center gap-1.5">
              <ClockIcon className="w-4 h-4 text-gray-400" />
              {formatDateTime(meeting.scheduledAt)}
            </span>
            {meeting.duration && (
              <span className="text-gray-300">|</span>
            )}
            {meeting.duration && (
              <span>{formatDuration(meeting.duration)}</span>
            )}
          </div>
          
          {/* Participants */}
          {meeting.participants.length > 0 && (
            <div className="flex items-center">
              <div className="flex -space-x-2">
                {meeting.participants.slice(0, 3).map((p, i) => (
                  <div
                    key={p.id}
                    className="w-7 h-7 rounded-full bg-gradient-to-br from-indigo-500 to-violet-500 flex items-center justify-center text-white text-xs font-medium ring-2 ring-white"
                    title={p.userName}
                  >
                    {p.userName?.charAt(0).toUpperCase() || 'U'}
                  </div>
                ))}
                {meeting.participants.length > 3 && (
                  <div className="w-7 h-7 rounded-full bg-gray-100 flex items-center justify-center text-xs font-medium text-gray-600 ring-2 ring-white">
                    +{meeting.participants.length - 3}
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      </div>
    </Link>
  );
}

function ClockIcon({ className }: { className?: string }) {
  return (
    <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
  );
}

function PlanningIcon() {
  return (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" />
    </svg>
  );
}

function ReviewIcon() {
  return (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
    </svg>
  );
}

function StandupIcon() {
  return (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
    </svg>
  );
}

function RetroIcon() {
  return (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
    </svg>
  );
}

function OneOnOneIcon() {
  return (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
    </svg>
  );
}

function OtherIcon() {
  return (
    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
    </svg>
  );
}
