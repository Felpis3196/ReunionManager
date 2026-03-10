import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { authService, UserInfo, LoginDto, RegisterDto } from '@/services/api';

function normalizeUser(u: any): UserInfo {
  return {
    id: u?.id ?? u?.Id ?? '',
    name: u?.name ?? u?.Name ?? '',
    email: u?.email ?? u?.Email ?? '',
    avatarUrl: u?.avatarUrl ?? u?.AvatarUrl,
    role: u?.role ?? u?.Role ?? 'Member',
    isSiteAdmin: u?.isSiteAdmin ?? u?.IsSiteAdmin ?? false,
    organizationId: u?.organizationId ?? u?.OrganizationId,
    organizationName: u?.organizationName ?? u?.OrganizationName,
    createdAt: u?.createdAt ?? u?.CreatedAt ?? new Date().toISOString(),
    canInviteMembers: u?.canInviteMembers ?? u?.CanInviteMembers ?? false,
    canManageRoles: u?.canManageRoles ?? u?.CanManageRoles ?? false,
    canRemoveMembers: u?.canRemoveMembers ?? u?.CanRemoveMembers ?? false,
    canManageTasks: u?.canManageTasks ?? u?.CanManageTasks ?? false,
    canAssignTasks: u?.canAssignTasks ?? u?.CanAssignTasks ?? false,
    canCompleteAnyTask: u?.canCompleteAnyTask ?? u?.CanCompleteAnyTask ?? false,
    canViewAllTasks: u?.canViewAllTasks ?? u?.CanViewAllTasks ?? false,
  };
}

interface AuthState {
  user: UserInfo | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  _hasHydrated: boolean;

  login: (credentials: LoginDto) => Promise<boolean>;
  register: (data: RegisterDto) => Promise<boolean>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<boolean>;
  updateProfile: (data: { name?: string; avatarUrl?: string }) => Promise<boolean>;
  clearError: () => void;
  checkAuth: () => Promise<void>;
  setHasHydrated: (value: boolean) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,
      _hasHydrated: false,

      setHasHydrated: (value) => set({ _hasHydrated: value }),

      login: async (credentials) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.login(credentials);
          const success = response?.success === true;
          const user = response?.user ?? (response as any)?.User;
          const accessToken = response?.accessToken ?? (response as any)?.AccessToken;
          const message = response?.message ?? (response as any)?.Message;
          if (success && user && accessToken) {
            set({
              user: normalizeUser(user),
              accessToken,
              isAuthenticated: true,
              isLoading: false,
              error: null,
            });
            return true;
          } else {
            set({
              error: message || 'Erro ao fazer login',
              isLoading: false,
            });
            return false;
          }
        } catch (error: any) {
          const data = error.response?.data;
          const message = data?.message ?? data?.Message
            ?? data?.error ?? data?.Error
            ?? (Array.isArray(data?.details) ? data.details.join('; ') : null)
            ?? error.message
            ?? 'Erro ao fazer login. Verifique email e senha.';
          set({
            error: String(message),
            isLoading: false,
          });
          return false;
        }
      },

      register: async (data) => {
        set({ isLoading: true, error: null });
        try {
          const response = await authService.register(data);
          const success = response?.success === true;
          const user = response?.user ?? (response as any)?.User;
          const accessToken = response?.accessToken ?? (response as any)?.AccessToken;
          const message = response?.message ?? (response as any)?.Message;
          if (success && user && accessToken) {
            set({
              user: normalizeUser(user),
              accessToken,
              isAuthenticated: true,
              isLoading: false,
              error: null,
            });
            return true;
          } else {
            set({
              error: message || 'Erro ao criar conta',
              isLoading: false,
            });
            return false;
          }
        } catch (error: any) {
          const data = error.response?.data;
          const message = data?.message ?? data?.Message
            ?? data?.error ?? data?.Error
            ?? (Array.isArray(data?.details) ? data.details.join('; ') : null)
            ?? error.message
            ?? 'Erro ao criar conta. Verifique os dados e tente novamente.';
          const detail = data?.errorDetail ?? data?.ErrorDetail;
          set({
            error: detail ? `${message}\n${detail}` : String(message),
            isLoading: false,
          });
          return false;
        }
      },

      logout: async () => {
        try {
          await authService.logout();
        } catch (error) {
          console.error('Error during logout:', error);
        } finally {
          set({
            user: null,
            accessToken: null,
            isAuthenticated: false,
          });
        }
      },

      refreshToken: async () => {
        try {
          const response = await authService.refreshToken();
          const success = response?.success === true;
          const accessToken = response?.accessToken ?? (response as any)?.AccessToken;
          const user = response?.user ?? (response as any)?.User;
          if (success && accessToken) {
            set({
              accessToken,
              user: user ? normalizeUser(user) : get().user,
            });
            return true;
          }
          return false;
        } catch (error) {
          set({
            user: null,
            accessToken: null,
            isAuthenticated: false,
          });
          return false;
        }
      },

      updateProfile: async (data) => {
        set({ isLoading: true, error: null });
        try {
          const user = await authService.updateProfile(data);
          if (user) {
            set({ user: normalizeUser(user), isLoading: false });
            return true;
          }
          return false;
        } catch (error: any) {
          set({
            error: error.response?.data?.message || 'Erro ao atualizar perfil',
            isLoading: false,
          });
          return false;
        }
      },

      clearError: () => set({ error: null }),

      checkAuth: async () => {
        const { accessToken } = get();
        if (!accessToken) {
          set({ isAuthenticated: false });
          return;
        }

        try {
          const user = await authService.getCurrentUser();
          if (user) {
            set({ user: normalizeUser(user), isAuthenticated: true });
          } else {
            // Try to refresh
            const refreshed = await get().refreshToken();
            if (!refreshed) {
              set({ user: null, accessToken: null, isAuthenticated: false });
            }
          }
        } catch (error) {
          // Try to refresh on error
          const refreshed = await get().refreshToken();
          if (!refreshed) {
            set({ user: null, accessToken: null, isAuthenticated: false });
          }
        }
      },
    }),
    {
      name: 'auth-storage',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({
        accessToken: state.accessToken,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
      onRehydrateStorage: () => (state) => {
        state?.setHasHydrated(true);
      },
    }
  )
);
