const THEME_KEY = 'smm-theme';
const EMAIL_REMINDERS_KEY = 'smm-email-reminders';
const NOTIFY_NEW_TASKS_KEY = 'smm-notify-new-tasks';
const DATE_FORMAT_KEY = 'smm-date-format';

export type ThemeValue = 'light' | 'dark' | 'system';

export function getTheme(): ThemeValue {
  if (typeof window === 'undefined') return 'system';
  const stored = window.localStorage.getItem(THEME_KEY);
  if (stored === 'light' || stored === 'dark' || stored === 'system') return stored;
  return 'system';
}

export function setTheme(value: ThemeValue): void {
  if (typeof window === 'undefined') return;
  window.localStorage.setItem(THEME_KEY, value);
  applyTheme();
  window.dispatchEvent(new CustomEvent('theme-change'));
}

/** Whether the UI is currently in dark mode (for toggles / icons). */
export function isDarkMode(): boolean {
  if (typeof document === 'undefined') return false;
  return document.documentElement.classList.contains('dark');
}

export function applyTheme(): void {
  if (typeof document === 'undefined') return;
  const theme = getTheme();
  const isDark =
    theme === 'dark' ||
    (theme === 'system' &&
      typeof window !== 'undefined' &&
      window.matchMedia('(prefers-color-scheme: dark)').matches);
  document.documentElement.classList.toggle('dark', isDark);
}

export function getEmailReminders(): boolean {
  if (typeof window === 'undefined') return true;
  const v = window.localStorage.getItem(EMAIL_REMINDERS_KEY);
  return v !== 'false';
}

export function setEmailReminders(value: boolean): void {
  if (typeof window === 'undefined') return;
  window.localStorage.setItem(EMAIL_REMINDERS_KEY, value ? 'true' : 'false');
}

export function getNotifyNewTasks(): boolean {
  if (typeof window === 'undefined') return true;
  const v = window.localStorage.getItem(NOTIFY_NEW_TASKS_KEY);
  return v !== 'false';
}

export function setNotifyNewTasks(value: boolean): void {
  if (typeof window === 'undefined') return;
  window.localStorage.setItem(NOTIFY_NEW_TASKS_KEY, value ? 'true' : 'false');
}

export type DateFormatValue = 'pt-BR' | 'en-US';

export function getDateFormat(): DateFormatValue {
  if (typeof window === 'undefined') return 'pt-BR';
  const v = window.localStorage.getItem(DATE_FORMAT_KEY);
  return v === 'en-US' ? 'en-US' : 'pt-BR';
}

export function setDateFormat(value: DateFormatValue): void {
  if (typeof window === 'undefined') return;
  window.localStorage.setItem(DATE_FORMAT_KEY, value);
}
