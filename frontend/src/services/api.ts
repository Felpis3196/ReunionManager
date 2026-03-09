import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import type { Meeting, CreateMeetingDto, UpdateMeetingDto } from '@/types/meeting';

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 seconds
  withCredentials: true, // Enable cookies for refresh token
});

// Helper to get token from localStorage (client-side only)
const getAccessToken = (): string | null => {
  if (typeof window === 'undefined') return null;
  try {
    const stored = localStorage.getItem('auth-storage');
    if (stored) {
      const parsed = JSON.parse(stored);
      const state = parsed?.state ?? parsed?.State;
      return state?.accessToken ?? state?.AccessToken ?? null;
    }
  } catch (e) {
    console.error('Error getting access token:', e);
  }
  return null;
};

// Add request interceptor for auth and debugging
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = getAccessToken();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    console.log('API Request:', config.method?.toUpperCase(), config.url, config.data);
    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Evitar loop: quando o refresh falha (401), nao tentar refrescar de novo
let refreshPromise: Promise<string | null> | null = null;

function clearAuthAndRedirect() {
  if (typeof window === 'undefined') return;
  localStorage.removeItem('auth-storage');
  window.location.href = '/login';
}

// Add response interceptor for debugging and token refresh
api.interceptors.response.use(
  (response) => {
    console.log('API Response:', response.status, response.data);
    return response;
  },
  async (error: AxiosError) => {
    console.error('API Response Error:', {
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      message: error.message,
    });

    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    const url = originalRequest?.url ?? '';
    const isLoginOrRegister = url.includes('/api/auth/login') || url.includes('/api/auth/register');
    const isRefreshEndpoint = url.includes('/api/auth/refresh-token');

    // Se a propria chamada de refresh retornou 401: limpar e ir para login (evita loop infinito)
    if (error.response?.status === 401 && isRefreshEndpoint) {
      refreshPromise = null;
      clearAuthAndRedirect();
      return Promise.reject(error);
    }

    // Handle 401 - tentar refresh uma vez (nao para login/register)
    if (error.response?.status === 401 && !originalRequest?._retry && !isLoginOrRegister) {
      originalRequest._retry = true;

      if (!refreshPromise) {
        refreshPromise = (async () => {
          try {
            const response = await api.post('/api/auth/refresh-token');
            const data = response.data as any;
            const success = data?.success === true || data?.Success === true;
            const accessToken = data?.accessToken ?? data?.AccessToken ?? null;
            if (success && accessToken) {
              if (typeof window !== 'undefined') {
                const stored = localStorage.getItem('auth-storage');
                if (stored) {
                  const parsed = JSON.parse(stored);
                  if (parsed.state) parsed.state.accessToken = accessToken;
                  localStorage.setItem('auth-storage', JSON.stringify(parsed));
                }
              }
              return accessToken;
            }
            return null;
          } catch {
            return null;
          } finally {
            refreshPromise = null;
          }
        })();
      }

      const accessToken = await refreshPromise;
      if (accessToken && originalRequest.headers) {
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      }

      clearAuthAndRedirect();
    }

    return Promise.reject(error);
  }
);

export const meetingService = {
  async getAll(): Promise<Meeting[]> {
    const response = await api.get<Meeting[]>('/api/meetings');
    return response.data;
  },

  async getById(id: string): Promise<Meeting> {
    const response = await api.get<Meeting>(`/api/meetings/${id}`);
    return response.data;
  },

  async create(dto: CreateMeetingDto): Promise<Meeting> {
    const response = await api.post<Meeting>('/api/meetings', dto);
    return response.data;
  },

  async update(id: string, dto: UpdateMeetingDto): Promise<Meeting> {
    const response = await api.put<Meeting>(`/api/meetings/${id}`, dto);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/api/meetings/${id}`);
  },

  async addParticipant(meetingId: string, userId: string): Promise<void> {
    await api.post(`/api/meetings/${meetingId}/participants`, { userId });
  },

  async removeParticipant(meetingId: string, participantId: string): Promise<void> {
    await api.delete(`/api/meetings/${meetingId}/participants/${participantId}`);
  },

  async updateParticipantStatus(meetingId: string, participantId: string, status: string): Promise<void> {
    await api.patch(`/api/meetings/${meetingId}/participants/${participantId}`, { status });
  },

  async generateAgenda(id: string): Promise<{ agenda: string }> {
    const response = await api.post<{ agenda: string }>(`/api/meetings/${id}/generate-agenda`);
    return response.data;
  },

  async processTranscript(id: string, transcript: string): Promise<void> {
    await api.post(`/api/meetings/${id}/process-transcript`, { transcript });
  },

  async startMeeting(id: string): Promise<Meeting> {
    const response = await api.post<Meeting>(`/api/meetings/${id}/start`);
    return response.data;
  },

  async endMeeting(id: string): Promise<Meeting> {
    const response = await api.post<Meeting>(`/api/meetings/${id}/end`);
    return response.data;
  },

  async cancelMeeting(id: string): Promise<Meeting> {
    const response = await api.post<Meeting>(`/api/meetings/${id}/cancel`);
    return response.data;
  },
};

// User service for participants
export const userService = {
  async getAll(): Promise<{ id: string; name: string; email: string }[]> {
    const response = await api.get('/api/users');
    return response.data;
  },

  async search(query: string): Promise<{ id: string; name: string; email: string }[]> {
    const response = await api.get(`/api/users/search?q=${encodeURIComponent(query)}`);
    return response.data;
  },
};

// Dashboard service
export const dashboardService = {
  async getStats(organizationId?: string): Promise<DashboardStats> {
    const params = organizationId ? `?organizationId=${organizationId}` : '';
    const response = await api.get(`/api/dashboard/stats${params}`);
    return response.data;
  },

  async getProductivity(days: number = 30, organizationId?: string): Promise<ProductivityStats> {
    const params = new URLSearchParams({ days: String(days) });
    if (organizationId) params.append('organizationId', organizationId);
    const response = await api.get(`/api/dashboard/productivity?${params.toString()}`);
    return response.data;
  },
};

// Tasks service
export const taskService = {
  async getAll(filters?: { meetingId?: string; assignedToId?: string; status?: string }): Promise<Task[]> {
    const params = new URLSearchParams();
    if (filters?.meetingId) params.append('meetingId', filters.meetingId);
    if (filters?.assignedToId) params.append('assignedToId', filters.assignedToId);
    if (filters?.status) params.append('status', filters.status);
    const query = params.toString();
    const response = await api.get(`/api/tasks${query ? `?${query}` : ''}`);
    return response.data;
  },

  async getById(id: string): Promise<Task> {
    const response = await api.get(`/api/tasks/${id}`);
    return response.data;
  },

  async create(task: CreateTaskDto): Promise<Task> {
    const response = await api.post('/api/tasks', task);
    return response.data;
  },

  async update(id: string, task: Partial<Task>): Promise<Task> {
    const response = await api.put(`/api/tasks/${id}`, task);
    return response.data;
  },

  async complete(id: string): Promise<Task> {
    const response = await api.post(`/api/tasks/${id}/complete`);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/api/tasks/${id}`);
  },
};

// Decisions service
export const decisionService = {
  async getAll(meetingId: string): Promise<Decision[]> {
    const response = await api.get(`/api/meetings/${meetingId}/decisions`);
    return response.data;
  },

  async create(meetingId: string, decision: { title: string; description: string; madeById?: string }): Promise<Decision> {
    const response = await api.post(`/api/meetings/${meetingId}/decisions`, decision);
    return response.data;
  },

  async update(meetingId: string, id: string, data: Partial<Decision>): Promise<Decision> {
    const response = await api.put(`/api/meetings/${meetingId}/decisions/${id}`, data);
    return response.data;
  },

  async markAsImplemented(meetingId: string, id: string): Promise<Decision> {
    const response = await api.post(`/api/meetings/${meetingId}/decisions/${id}/implement`);
    return response.data;
  },

  async delete(meetingId: string, id: string): Promise<void> {
    await api.delete(`/api/meetings/${meetingId}/decisions/${id}`);
  },
};

// Agenda service
export const agendaService = {
  async getAll(meetingId: string): Promise<AgendaItem[]> {
    const response = await api.get(`/api/meetings/${meetingId}/agenda`);
    return response.data;
  },

  async create(meetingId: string, item: { title: string; description?: string; estimatedMinutes?: number }): Promise<AgendaItem> {
    const response = await api.post(`/api/meetings/${meetingId}/agenda`, item);
    return response.data;
  },

  async update(meetingId: string, id: string, data: Partial<AgendaItem>): Promise<AgendaItem> {
    const response = await api.put(`/api/meetings/${meetingId}/agenda/${id}`, data);
    return response.data;
  },

  async markAsComplete(meetingId: string, id: string): Promise<AgendaItem> {
    const response = await api.post(`/api/meetings/${meetingId}/agenda/${id}/complete`);
    return response.data;
  },

  async reorder(meetingId: string, items: { id: string; order: number }[]): Promise<void> {
    await api.post(`/api/meetings/${meetingId}/agenda/reorder`, items);
  },

  async delete(meetingId: string, id: string): Promise<void> {
    await api.delete(`/api/meetings/${meetingId}/agenda/${id}`);
  },
};

// Types
export interface DashboardStats {
  totalMeetings: number;
  meetingsThisMonth: number;
  meetingsLastMonth: number;
  scheduledMeetings: number;
  completedMeetings: number;
  cancelledMeetings: number;
  inProgressMeetings: number;
  totalTasks: number;
  pendingTasks: number;
  inProgressTasks: number;
  completedTasks: number;
  taskCompletionRate: number;
  totalMeetingHours: number;
  averageMeetingDuration: string;
  upcomingMeetings: UpcomingMeetingSummary[];
  recentMeetings: RecentMeetingSummary[];
  meetingsByType: { type: string; count: number }[];
  tasksByPriority: { priority: string; count: number }[];
}

export interface UpcomingMeetingSummary {
  id: string;
  title: string;
  scheduledAt: string;
  type: string;
  participantCount: number;
}

export interface RecentMeetingSummary {
  id: string;
  title: string;
  completedAt: string;
  duration: string;
  decisionCount: number;
  taskCount: number;
}

export interface ProductivityStats {
  period: string;
  totalMeetings: number;
  completedMeetings: number;
  totalHoursInMeetings: number;
  averageDecisionsPerMeeting: number;
  averageTasksPerMeeting: number;
  meetingsWithDecisions: number;
  meetingsWithTasks: number;
  dailyStats: { date: string; meetingCount: number; totalMinutes: number }[];
}

export interface Task {
  id: string;
  meetingId: string;
  projectId?: string;
  assignedToId: string;
  title: string;
  description?: string;
  status: string;
  priority: string;
  dueDate?: string;
  completedAt?: string;
  createdAt: string;
}

export interface CreateTaskDto {
  meetingId: string;
  projectId?: string;
  assignedToId: string;
  title: string;
  description?: string;
  priority?: string;
  dueDate?: string;
}

export interface Decision {
  id: string;
  meetingId: string;
  title: string;
  description: string;
  madeById?: string;
  madeAt: string;
  isImplemented: boolean;
}

export interface AgendaItem {
  id: string;
  meetingId: string;
  order: number;
  title: string;
  description?: string;
  estimatedMinutes?: number;
  isCompleted: boolean;
}

// Auth Types
export interface LoginDto {
  email: string;
  password: string;
  rememberMe?: boolean;
}

export interface RegisterDto {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  organizationName?: string;
  inviteCode?: string;
  invitePassword?: string;
}

export interface AuthResponse {
  success: boolean;
  message?: string;
  accessToken?: string;
  refreshToken?: string;
  expiresAt?: string;
  user?: UserInfo;
}

export interface UserInfo {
  id: string;
  name: string;
  email: string;
  avatarUrl?: string;
  role: string;
  isSiteAdmin: boolean;
  organizationId?: string;
  organizationName?: string;
  createdAt: string;
  canInviteMembers?: boolean;
  canManageRoles?: boolean;
  canRemoveMembers?: boolean;
}

export interface MyOrganizationItem {
  id: string;
  name: string;
  role: string;
}

export interface OrganizationMemberInfo {
  userId: string;
  name: string;
  email: string;
  avatarUrl?: string;
  role: string;
}

export interface InviteDto {
  email: string;
  role?: string;
  customRoleId?: string | null;
  invitePassword?: string;
}

export interface OrganizationRoleDto {
  id: string;
  organizationId: string;
  name: string;
  permissions: string[];
}

export interface CreateOrganizationRoleDto {
  name: string;
  permissions: string[];
}

export interface UpdateOrganizationRoleDto {
  name: string;
  permissions: string[];
}

export interface InviteResponse {
  id: string;
  email: string;
  inviteCode: string;
  status: string;
  expiresAt: string;
  hasPassword?: boolean;
}

// Files service
export const fileService = {
  async getAll(meetingId: string): Promise<MeetingFile[]> {
    const response = await api.get(`/api/meetings/${meetingId}/files`);
    return response.data;
  },

  async upload(meetingId: string, file: File, description?: string): Promise<MeetingFile> {
    const formData = new FormData();
    formData.append('file', file);
    if (description) {
      formData.append('description', description);
    }
    const response = await api.post(`/api/meetings/${meetingId}/files`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  async download(meetingId: string, fileId: string): Promise<Blob> {
    const response = await api.get(`/api/meetings/${meetingId}/files/${fileId}/download`, {
      responseType: 'blob',
    });
    return response.data;
  },

  async delete(meetingId: string, fileId: string): Promise<void> {
    await api.delete(`/api/meetings/${meetingId}/files/${fileId}`);
  },

  getDownloadUrl(meetingId: string, fileId: string): string {
    const baseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';
    return `${baseUrl}/api/meetings/${meetingId}/files/${fileId}/download`;
  },
};

export interface MeetingFile {
  id: string;
  meetingId: string;
  uploadedById: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  category: string;
  description?: string;
  createdAt: string;
}

// Auth Service
export const authService = {
  async login(credentials: LoginDto): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/login', credentials);
    return response.data;
  },

  async register(data: RegisterDto): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/register', data);
    return response.data;
  },

  async logout(): Promise<void> {
    await api.post('/api/auth/logout');
  },

  async refreshToken(): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/refresh-token');
    return response.data;
  },

  async getCurrentUser(): Promise<UserInfo | null> {
    try {
      const response = await api.get<UserInfo>('/api/auth/me');
      return response.data;
    } catch (error) {
      return null;
    }
  },

  async updateProfile(data: { name?: string; avatarUrl?: string }): Promise<UserInfo | null> {
    try {
      const response = await api.put<UserInfo>('/api/auth/me', data);
      return response.data;
    } catch (error) {
      return null;
    }
  },

  async getMyOrganizations(): Promise<MyOrganizationItem[]> {
    try {
      const response = await api.get<MyOrganizationItem[]>('/api/auth/my-organizations');
      return response.data;
    } catch (error) {
      return [];
    }
  },

  async getMyOrganizationMembers(): Promise<OrganizationMemberInfo[]> {
    try {
      const response = await api.get<OrganizationMemberInfo[]>('/api/auth/me/organization-members');
      return response.data;
    } catch (error) {
      return [];
    }
  },

  async changePassword(data: { currentPassword: string; newPassword: string; confirmNewPassword: string }): Promise<boolean> {
    try {
      await api.post('/api/auth/change-password', data);
      return true;
    } catch (error) {
      return false;
    }
  },

  async forgotPassword(email: string): Promise<void> {
    await api.post('/api/auth/forgot-password', { email });
  },

  async resetPassword(data: { token: string; newPassword: string; confirmNewPassword: string }): Promise<boolean> {
    try {
      await api.post('/api/auth/reset-password', data);
      return true;
    } catch (error) {
      return false;
    }
  },

  async inviteUser(data: InviteDto): Promise<InviteResponse> {
    const response = await api.post<InviteResponse>('/api/auth/invite', data);
    return response.data;
  },

  async getPendingInvites(): Promise<InviteResponse[]> {
    const response = await api.get<InviteResponse[]>('/api/auth/invites');
    return response.data;
  },

  async cancelInvite(id: string): Promise<void> {
    await api.delete(`/api/auth/invites/${id}`);
  },

  async removeOrganizationMember(memberUserId: string): Promise<void> {
    await api.delete(`/api/auth/me/organization-members/${memberUserId}`);
  },

  async getOrganizationRoles(): Promise<OrganizationRoleDto[]> {
    try {
      const response = await api.get<OrganizationRoleDto[]>('/api/auth/me/organization-roles');
      return response.data;
    } catch (error) {
      return [];
    }
  },

  async createOrganizationRole(data: CreateOrganizationRoleDto): Promise<OrganizationRoleDto> {
    const response = await api.post<OrganizationRoleDto>('/api/auth/me/organization-roles', data);
    return response.data;
  },

  async updateOrganizationRole(id: string, data: UpdateOrganizationRoleDto): Promise<OrganizationRoleDto> {
    const response = await api.put<OrganizationRoleDto>(`/api/auth/me/organization-roles/${id}`, data);
    return response.data;
  },

  async deleteOrganizationRole(id: string): Promise<void> {
    await api.delete(`/api/auth/me/organization-roles/${id}`);
  },

  async acceptInvite(data: { inviteCode: string; invitePassword?: string }): Promise<{ message: string }> {
    const response = await api.post<{ message: string }>('/api/auth/accept-invite', data);
    return response.data;
  },
};

// Notifications
export interface NotificationMeeting {
  id: string;
  title: string;
  scheduledAt: string;
  type: string;
}

export interface NotificationTask {
  id: string;
  title: string;
  dueDate?: string;
  status: string;
  priority: string;
  meetingId: string;
}

export interface NotificationsResponse {
  invites: InviteResponse[];
  meetingsStartingSoon: NotificationMeeting[];
  upcomingMeetings: NotificationMeeting[];
  tasksDueSoon: NotificationTask[];
  unreadCount: number;
}

export const notificationsService = {
  async getAll(): Promise<NotificationsResponse> {
    const response = await api.get<NotificationsResponse>('/api/notifications');
    return response.data;
  },
};

// Team chat (REST + SignalR for real-time)
export interface ChatMessageDto {
  id: string;
  userId: string;
  userName: string;
  userAvatarUrl?: string;
  text: string;
  createdAt: string;
}

export const chatService = {
  async getMessages(limit = 50): Promise<ChatMessageDto[]> {
    const response = await api.get<ChatMessageDto[]>('/api/chat/messages', { params: { limit } });
    return response.data ?? [];
  },

  async sendMessage(text: string): Promise<ChatMessageDto> {
    const response = await api.post<ChatMessageDto>('/api/chat/messages', { text });
    return response.data;
  },
};

/** Base URL for the API (used to build SignalR hub URL). */
export function getApiBaseUrl(): string {
  return process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';
}

/** Full URL for the team chat SignalR hub (append ?access_token=... when connecting). */
export function getChatHubUrl(): string {
  return getApiBaseUrl() + '/hubs/teamchat';
}

export default api;
