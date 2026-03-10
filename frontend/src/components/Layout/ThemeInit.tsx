'use client';

import { useEffect } from 'react';
import { applyTheme, getTheme } from '@/lib/theme';

export default function ThemeInit() {
  useEffect(() => {
    applyTheme();
    const theme = getTheme();
    if (theme !== 'system') return;
    const mq = window.matchMedia('(prefers-color-scheme: dark)');
    const listener = () => applyTheme();
    mq.addEventListener('change', listener);
    return () => mq.removeEventListener('change', listener);
  }, []);
  return null;
}
