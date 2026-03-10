import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';
import AuthProvider from '@/components/Auth/AuthProvider';
import ThemeInit from '@/components/Layout/ThemeInit';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'Smart Meeting Manager',
  description: 'Sistema de Gestão de Reuniões Inteligente',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="pt-BR">
      <body className={inter.className}>
        <ThemeInit />
        <AuthProvider>
          {children}
        </AuthProvider>
      </body>
    </html>
  );
}
