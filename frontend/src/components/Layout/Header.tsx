import React from 'react';

export default function Header() {
  return (
    <header className="bg-white shadow-sm border-b">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <div className="flex items-center">
            <h1 className="text-2xl font-bold text-gray-900">
              Smart Meeting Manager
            </h1>
          </div>
          <nav className="flex items-center space-x-4">
            <a href="/" className="text-gray-600 hover:text-gray-900">
              Reuniões
            </a>
            <a href="/dashboard" className="text-gray-600 hover:text-gray-900">
              Dashboard
            </a>
            <a href="/tasks" className="text-gray-600 hover:text-gray-900">
              Tarefas
            </a>
          </nav>
        </div>
      </div>
    </header>
  );
}
