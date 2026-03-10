import React from 'react';
import Header from './Header';

interface LayoutProps {
  children: React.ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-slate-900">
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-[260px,minmax(0,1fr)] gap-6 items-start">
          <aside className="lg:sticky lg:top-6">
            <Header />
          </aside>
          <section className="min-w-0">
            {children}
          </section>
        </div>
      </main>
    </div>
  );
}
