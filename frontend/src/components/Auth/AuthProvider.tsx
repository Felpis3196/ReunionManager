'use client';

import React, { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { useAuthStore } from '@/stores/authStore';

const publicRoutes = ['/login', '/register', '/forgot-password', '/reset-password'];

interface AuthProviderProps {
  children: React.ReactNode;
}

export default function AuthProvider({ children }: AuthProviderProps) {
  const router = useRouter();
  const pathname = usePathname();
  const { isAuthenticated, checkAuth, isLoading, _hasHydrated } = useAuthStore();
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    if (!_hasHydrated) return;

    let cancelled = false;
    const timeoutId = setTimeout(() => {
      if (!cancelled) setIsChecking(false);
    }, 8000);

    const verifyAuth = async () => {
      try {
        await checkAuth();
      } catch (e) {
        console.error('Auth check error:', e);
      } finally {
        if (!cancelled) setIsChecking(false);
      }
    };
    verifyAuth();
    return () => {
      cancelled = true;
      clearTimeout(timeoutId);
    };
  }, [checkAuth, _hasHydrated]);

  useEffect(() => {
    if (isChecking) return;

    const isPublicRoute = publicRoutes.some(route => pathname?.startsWith(route));

    if (!isAuthenticated && !isPublicRoute) {
      router.push('/login');
    }

    if (isAuthenticated && isPublicRoute) {
      router.push('/');
    }
  }, [isAuthenticated, pathname, router, isChecking]);

  // Show loading while store is rehydrating or checking auth
  if (!_hasHydrated || isChecking) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-50 via-white to-indigo-50">
        <div className="flex flex-col items-center gap-4">
          <div className="w-14 h-14 bg-gradient-to-br from-indigo-600 to-violet-600 rounded-2xl flex items-center justify-center animate-pulse">
            <svg className="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
          <div className="flex items-center gap-2">
            <div className="w-2 h-2 bg-indigo-600 rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
            <div className="w-2 h-2 bg-indigo-600 rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
            <div className="w-2 h-2 bg-indigo-600 rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
          </div>
        </div>
      </div>
    );
  }

  // For public routes, always render
  const isPublicRoute = publicRoutes.some(route => pathname?.startsWith(route));
  if (isPublicRoute) {
    return <>{children}</>;
  }

  // For protected routes, only render if authenticated
  if (!isAuthenticated) {
    return null;
  }

  return <>{children}</>;
}
