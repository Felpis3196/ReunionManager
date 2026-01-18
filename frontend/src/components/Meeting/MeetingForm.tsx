'use client';

import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { CreateMeetingDto, MeetingType } from '@/types/meeting';

const meetingSchema = z.object({
  title: z.string().min(1, 'Título é obrigatório'),
  description: z.string().optional(),
  type: z.nativeEnum(MeetingType),
  scheduledAt: z.string().min(1, 'Data/hora é obrigatória'),
  duration: z.string().min(1, 'Duração é obrigatória'),
  location: z.string().optional(),
  meetingUrl: z.string().url().optional().or(z.literal('')),
  participantIds: z.array(z.string()).optional(),
});

interface MeetingFormProps {
  onSubmit: (data: CreateMeetingDto) => Promise<void>;
  isLoading?: boolean;
}

export default function MeetingForm({ onSubmit, isLoading }: MeetingFormProps) {
  const { register, handleSubmit, formState: { errors } } = useForm<CreateMeetingDto>({
    resolver: zodResolver(meetingSchema),
    defaultValues: {
      type: MeetingType.Other,
      participantIds: [],
    },
  });

  const onFormSubmit = async (data: CreateMeetingDto) => {
    await onSubmit({
      ...data,
      organizationId: '00000000-0000-0000-0000-000000000000', // TODO: Get from auth context
    });
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-6">
      <div>
        <label htmlFor="title" className="block text-sm font-medium text-gray-700">
          Título *
        </label>
        <input
          {...register('title')}
          type="text"
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
        />
        {errors.title && <p className="mt-1 text-sm text-red-600">{errors.title.message}</p>}
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700">
          Descrição
        </label>
        <textarea
          {...register('description')}
          rows={3}
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="type" className="block text-sm font-medium text-gray-700">
            Tipo *
          </label>
          <select
            {...register('type')}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
          >
            {Object.values(MeetingType).map((type) => (
              <option key={type} value={type}>
                {type}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label htmlFor="duration" className="block text-sm font-medium text-gray-700">
            Duração *
          </label>
          <input
            {...register('duration')}
            type="time"
            step="900"
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
          />
          {errors.duration && <p className="mt-1 text-sm text-red-600">{errors.duration.message}</p>}
        </div>
      </div>

      <div>
        <label htmlFor="scheduledAt" className="block text-sm font-medium text-gray-700">
          Data e Hora *
        </label>
        <input
          {...register('scheduledAt')}
          type="datetime-local"
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
        />
        {errors.scheduledAt && <p className="mt-1 text-sm text-red-600">{errors.scheduledAt.message}</p>}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label htmlFor="location" className="block text-sm font-medium text-gray-700">
            Localização
          </label>
          <input
            {...register('location')}
            type="text"
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
          />
        </div>

        <div>
          <label htmlFor="meetingUrl" className="block text-sm font-medium text-gray-700">
            URL da Reunião
          </label>
          <input
            {...register('meetingUrl')}
            type="url"
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500"
          />
        </div>
      </div>

      <div className="flex justify-end space-x-4">
        <button
          type="button"
          className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
        >
          Cancelar
        </button>
        <button
          type="submit"
          disabled={isLoading}
          className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 disabled:opacity-50"
        >
          {isLoading ? 'Salvando...' : 'Criar Reunião'}
        </button>
      </div>
    </form>
  );
}
