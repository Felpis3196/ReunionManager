import axios from 'axios';
import type { Meeting, CreateMeetingDto } from '@/types/meeting';

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
});

export const meetingService = {
  async getById(id: string): Promise<Meeting> {
    const response = await api.get<Meeting>(`/api/meetings/${id}`);
    return response.data;
  },

  async create(dto: CreateMeetingDto): Promise<Meeting> {
    const response = await api.post<Meeting>('/api/meetings', dto);
    return response.data;
  },

  async generateAgenda(id: string): Promise<{ agenda: string }> {
    const response = await api.post<{ agenda: string }>(`/api/meetings/${id}/generate-agenda`);
    return response.data;
  },

  async processTranscript(id: string, transcript: string): Promise<void> {
    await api.post(`/api/meetings/${id}/process-transcript`, { transcript });
  },
};

export default api;
