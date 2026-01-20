import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(d);
}

export function formatDuration(duration: string): string {
  // Parse ISO 8601 duration (PT1H30M) or TimeSpan format (01:30:00) or HH:mm format
  // Try ISO 8601 format first
  const isoMatch = duration.match(/PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?/);
  if (isoMatch) {
    const hours = parseInt(isoMatch[1] || '0');
    const minutes = parseInt(isoMatch[2] || '0');
    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    }
    return `${minutes}m`;
  }
  
  // Try TimeSpan format (HH:mm:ss or HH:mm)
  const timeSpanMatch = duration.match(/(\d{1,2}):(\d{2})(?::(\d{2}))?/);
  if (timeSpanMatch) {
    const hours = parseInt(timeSpanMatch[1] || '0');
    const minutes = parseInt(timeSpanMatch[2] || '0');
    if (hours > 0) {
      return `${hours}h ${minutes}m`;
    }
    return `${minutes}m`;
  }
  
  // Return as-is if format is not recognized
  return duration;
}
