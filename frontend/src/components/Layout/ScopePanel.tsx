'use client';

import React from 'react';
import { useAuthStore } from '@/stores/authStore';

export default function ScopePanel() {
  const { user } = useAuthStore();

  if (!user) return null;

  const isSiteAdmin = user.isSiteAdmin ?? false;
  const hasOrg = !!user.organizationId;

  const title = isSiteAdmin ? 'Escopo global' : 'Organização atual';
  const label = isSiteAdmin
    ? 'Visão global (admin)'
    : user.organizationName ?? 'Sem organização';
  const description = isSiteAdmin
    ? 'Você está vendo dados de todas as organizações da plataforma.'
    : hasOrg
      ? 'Os dados abaixo consideram apenas esta organização.'
      : 'Entre em uma organização para ver dados específicos.';

  return (
    <aside className="card p-4 lg:p-5 h-full flex flex-col gap-3 bg-white/80 border border-gray-100">
      <div className="inline-flex items-center gap-2 rounded-full bg-indigo-50 px-3 py-1 text-xs font-medium text-indigo-700 w-fit">
        <span className="inline-flex h-2 w-2 rounded-full bg-emerald-500" />
        <span>{title}</span>
      </div>
      <div>
        <p className="text-sm font-semibold text-gray-900 truncate" title={label}>
          {label}
        </p>
        {user.role && (
          <p className="mt-0.5 text-xs text-gray-500">
            Papel: <span className="font-medium">{user.role}</span>
          </p>
        )}
      </div>
      <p className="text-xs text-gray-500 leading-relaxed">
        {description}
      </p>
    </aside>
  );
}
