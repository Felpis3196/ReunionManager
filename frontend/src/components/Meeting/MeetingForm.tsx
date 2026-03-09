'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { CreateMeetingDto, MeetingType } from '@/types/meeting';

const meetingSchema = z.object({
  title: z.string().min(1, 'Titulo e obrigatorio').max(200, 'Titulo deve ter no maximo 200 caracteres'),
  description: z.string().max(2000, 'Descricao deve ter no maximo 2000 caracteres').optional(),
  type: z.nativeEnum(MeetingType),
  scheduledAt: z.string().min(1, 'Data/hora e obrigatoria'),
  duration: z.string().min(1, 'Duracao e obrigatoria'),
  location: z.string().max(500, 'Localizacao deve ter no maximo 500 caracteres').optional(),
  meetingUrl: z.string().url('URL deve ser valida').max(1000, 'URL deve ter no maximo 1000 caracteres').optional().or(z.literal('')),
  participantIds: z.array(z.string()).optional(),
});

type MeetingFormData = z.infer<typeof meetingSchema>;

interface MeetingFormProps {
  onSubmit: (data: CreateMeetingDto) => Promise<void>;
  isLoading?: boolean;
}

const meetingTypeConfig: Record<MeetingType, { label: string; icon: React.ReactNode }> = {
  [MeetingType.Planning]: { label: 'Planejamento', icon: <ClipboardIcon /> },
  [MeetingType.Review]: { label: 'Revisao', icon: <CheckCircleIcon /> },
  [MeetingType.Standup]: { label: 'Daily Standup', icon: <UsersIcon /> },
  [MeetingType.Retrospective]: { label: 'Retrospectiva', icon: <RefreshIcon /> },
  [MeetingType.OneOnOne]: { label: '1:1', icon: <UserIcon /> },
  [MeetingType.Other]: { label: 'Outro', icon: <ChatIcon /> },
};

const durationOptions = [
  { value: '00:05', label: '5 min' },
  { value: '00:10', label: '10 min' },
  { value: '00:15', label: '15 min' },
  { value: '00:20', label: '20 min' },
  { value: '00:30', label: '30 min' },
  { value: '00:45', label: '45 min' },
  { value: '01:00', label: '1h' },
  { value: '01:30', label: '1h 30m' },
  { value: '02:00', label: '2h' },
  { value: '02:30', label: '2h 30m' },
  { value: '03:00', label: '3h' },
  { value: '04:00', label: '4h' },
];

export default function MeetingForm({ onSubmit, isLoading }: MeetingFormProps) {
  const [serverError, setServerError] = useState<string | null>(null);
  
  const { register, handleSubmit, formState: { errors }, watch, setValue } = useForm<MeetingFormData>({
    resolver: zodResolver(meetingSchema),
    defaultValues: {
      type: MeetingType.Other,
      participantIds: [],
      duration: '01:00',
    },
  });

  const selectedType = watch('type');
  const selectedDuration = watch('duration');

  const onFormSubmit = async (data: MeetingFormData) => {
    setServerError(null);
    try {
      const organizationId = '11111111-1111-1111-1111-111111111111';
      await onSubmit({
        ...data,
        organizationId,
        participantIds: data.participantIds || [],
      });
    } catch (error: any) {
      let errorMessage = 'Erro ao criar reuniao. Tente novamente.';
      if (error.response?.data) {
        const errorData = error.response.data;
        if (errorData.message) errorMessage = errorData.message;
        else if (errorData.details && Array.isArray(errorData.details)) errorMessage = errorData.details.join('\n');
        else if (errorData.error) errorMessage = errorData.error;
        else if (typeof errorData === 'string') errorMessage = errorData;
      } else if (error.message) errorMessage = error.message;
      setServerError(errorMessage);
    }
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-8">
      {/* Error Alert */}
      {serverError && (
        <div className="card border-red-100 bg-red-50 p-4 animate-scaleIn">
          <div className="flex items-start gap-3">
            <div className="flex-shrink-0 w-9 h-9 rounded-lg bg-red-100 flex items-center justify-center">
              <AlertIcon className="w-5 h-5 text-red-600" />
            </div>
            <div className="flex-1 min-w-0">
              <h3 className="text-sm font-medium text-red-800">Erro ao criar reuniao</h3>
              <p className="mt-1 text-sm text-red-600 whitespace-pre-line">{serverError}</p>
            </div>
            <button
              type="button"
              onClick={() => setServerError(null)}
              className="btn-icon text-red-400 hover:text-red-600 hover:bg-red-100"
            >
              <XIcon className="w-5 h-5" />
            </button>
          </div>
        </div>
      )}

      {/* Title */}
      <div>
        <label className="input-label">
          Titulo <span className="text-red-500">*</span>
        </label>
        <input
          {...register('title')}
          type="text"
          placeholder="Ex: Reuniao de planejamento sprint 15"
          className={errors.title ? 'border-red-300 focus:border-red-500 focus:ring-red-500/20' : ''}
        />
        {errors.title && <p className="input-error">{errors.title.message}</p>}
      </div>

      {/* Description */}
      <div>
        <label className="input-label">Descricao</label>
        <textarea
          {...register('description')}
          rows={3}
          placeholder="Descreva o objetivo e topicos da reuniao..."
          className={errors.description ? 'border-red-300 focus:border-red-500 focus:ring-red-500/20' : ''}
        />
        {errors.description && <p className="input-error">{errors.description.message}</p>}
      </div>

      {/* Meeting Type - Visual Selection */}
      <div>
        <label className="input-label">Tipo de Reuniao <span className="text-red-500">*</span></label>
        <input type="hidden" {...register('type')} />
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-3 mt-2">
          {Object.entries(meetingTypeConfig).map(([value, config]) => (
            <button
              key={value}
              type="button"
              onClick={() => setValue('type', value as MeetingType)}
              className={`flex items-center gap-3 p-3 rounded-xl border-2 transition-all duration-200 ${
                selectedType === value
                  ? 'border-indigo-500 bg-indigo-50 text-indigo-700'
                  : 'border-gray-200 hover:border-gray-300 text-gray-600 hover:bg-gray-50'
              }`}
            >
              <div className={`w-9 h-9 rounded-lg flex items-center justify-center ${
                selectedType === value ? 'bg-indigo-100' : 'bg-gray-100'
              }`}>
                {config.icon}
              </div>
              <span className="text-sm font-medium">{config.label}</span>
            </button>
          ))}
        </div>
      </div>

      {/* Date/Time and Duration - Side by side on larger screens */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className="input-label">
            Data e Hora <span className="text-red-500">*</span>
          </label>
          <input
            {...register('scheduledAt')}
            type="datetime-local"
            className={errors.scheduledAt ? 'border-red-300 focus:border-red-500 focus:ring-red-500/20' : ''}
          />
          {errors.scheduledAt && <p className="input-error">{errors.scheduledAt.message}</p>}
        </div>

        <div>
          <label className="input-label">Duracao <span className="text-red-500">*</span></label>
          <input type="hidden" {...register('duration')} />
          <div className="grid grid-cols-4 sm:grid-cols-6 gap-2 mt-2">
            {durationOptions.map((opt) => (
              <button
                key={opt.value}
                type="button"
                onClick={() => setValue('duration', opt.value)}
                className={`py-2 px-3 rounded-lg text-sm font-medium transition-all duration-200 ${
                  selectedDuration === opt.value
                    ? 'bg-indigo-600 text-white shadow-sm'
                    : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
                }`}
              >
                {opt.label}
              </button>
            ))}
          </div>
          {errors.duration && <p className="input-error">{errors.duration.message}</p>}
        </div>
      </div>

      {/* Location and URL */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className="input-label">Localizacao</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 w-12 flex items-center justify-center pointer-events-none text-gray-400">
              <LocationIcon className="w-5 h-5" />
            </div>
            <input
              {...register('location')}
              type="text"
              placeholder="Ex: Sala de reuniao 3A"
              className={`!pl-12 ${errors.location ? 'border-red-300 focus:border-red-500 focus:ring-red-500/20' : ''}`}
            />
          </div>
          {errors.location && <p className="input-error">{errors.location.message}</p>}
        </div>

        <div>
          <label className="input-label">Link da Reuniao Online</label>
          <div className="relative">
            <div className="absolute inset-y-0 left-0 w-12 flex items-center justify-center pointer-events-none text-gray-400">
              <LinkIcon className="w-5 h-5" />
            </div>
            <input
              {...register('meetingUrl')}
              type="url"
              placeholder="https://meet.google.com/xyz"
              className={`!pl-12 ${errors.meetingUrl ? 'border-red-300 focus:border-red-500 focus:ring-red-500/20' : ''}`}
            />
          </div>
          {errors.meetingUrl && <p className="input-error">{errors.meetingUrl.message}</p>}
        </div>
      </div>

      {/* Actions */}
      <div className="flex flex-col-reverse sm:flex-row justify-end gap-3 pt-6 border-t border-gray-100">
        <button
          type="button"
          onClick={() => window.history.back()}
          disabled={isLoading}
          className="btn-secondary"
        >
          Cancelar
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="btn-primary"
        >
          {isLoading ? (
            <>
              <LoadingSpinner className="w-4 h-4" />
              Criando...
            </>
          ) : (
            <>
              <PlusIcon className="w-5 h-5" />
              Criar Reuniao
            </>
          )}
        </button>
      </div>
    </form>
  );
}

// Icons
function ClipboardIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-3 7h3m-3 4h3m-6-4h.01M9 16h.01" /></svg>;
}
function CheckCircleIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>;
}
function UsersIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" /></svg>;
}
function RefreshIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>;
}
function UserIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>;
}
function ChatIcon() {
  return <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" /></svg>;
}
function LocationIcon({ className }: { className?: string }) {
  return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 11a3 3 0 11-6 0 3 3 0 016 0z" /></svg>;
}
function LinkIcon({ className }: { className?: string }) {
  return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" /></svg>;
}
function AlertIcon({ className }: { className?: string }) {
  return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>;
}
function XIcon({ className }: { className?: string }) {
  return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>;
}
function PlusIcon({ className }: { className?: string }) {
  return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>;
}
function LoadingSpinner({ className }: { className?: string }) {
  return <svg className={`animate-spin ${className}`} fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" /></svg>;
}
