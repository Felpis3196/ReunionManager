'use client';

import React, { useState } from 'react';
import Layout from '@/components/Layout/Layout';
import { useAuthStore } from '@/stores/authStore';
import { authService } from '@/services/api';

export default function ProfilePage() {
  const { user, updateProfile } = useAuthStore();
  const [isEditing, setIsEditing] = useState(false);
  const [isChangingPassword, setIsChangingPassword] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const [formData, setFormData] = useState({
    name: user?.name || '',
    avatarUrl: user?.avatarUrl || '',
  });

  const [passwordData, setPasswordData] = useState({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: '',
  });

  const handleSaveProfile = async () => {
    setIsSaving(true);
    setMessage(null);
    
    const success = await updateProfile(formData);
    
    if (success) {
      setMessage({ type: 'success', text: 'Perfil atualizado com sucesso!' });
      setIsEditing(false);
    } else {
      setMessage({ type: 'error', text: 'Erro ao atualizar perfil' });
    }
    
    setIsSaving(false);
  };

  const handleChangePassword = async () => {
    if (passwordData.newPassword !== passwordData.confirmNewPassword) {
      setMessage({ type: 'error', text: 'As senhas nao conferem' });
      return;
    }

    if (passwordData.newPassword.length < 6) {
      setMessage({ type: 'error', text: 'A nova senha deve ter pelo menos 6 caracteres' });
      return;
    }

    setIsSaving(true);
    setMessage(null);

    const success = await authService.changePassword({
      currentPassword: passwordData.currentPassword,
      newPassword: passwordData.newPassword,
      confirmNewPassword: passwordData.confirmNewPassword,
    });

    if (success) {
      setMessage({ type: 'success', text: 'Senha alterada com sucesso!' });
      setIsChangingPassword(false);
      setPasswordData({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
    } else {
      setMessage({ type: 'error', text: 'Erro ao alterar senha. Verifique a senha atual.' });
    }

    setIsSaving(false);
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-2xl mx-auto">
          {/* Header */}
          <div className="mb-8">
            <h1 className="page-title">Meu Perfil</h1>
            <p className="text-muted mt-1">Gerencie suas informacoes pessoais</p>
          </div>

          {/* Message */}
          {message && (
            <div className={`mb-6 p-4 rounded-xl border animate-fadeIn ${
              message.type === 'success' 
                ? 'bg-emerald-50 border-emerald-200 text-emerald-700' 
                : 'bg-red-50 border-red-200 text-red-700'
            }`}>
              <div className="flex items-center gap-2">
                {message.type === 'success' ? (
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                ) : (
                  <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                )}
                <span className="text-sm font-medium">{message.text}</span>
              </div>
            </div>
          )}

          {/* Profile Card */}
          <div className="card p-6 mb-6 animate-fadeIn">
            <div className="flex items-start gap-6">
              {/* Avatar */}
              <div className="relative">
                {user?.avatarUrl ? (
                  <img
                    src={user.avatarUrl}
                    alt={user.name}
                    className="w-24 h-24 rounded-2xl object-cover shadow-lg"
                  />
                ) : (
                  <div className="w-24 h-24 bg-gradient-to-br from-indigo-500 to-violet-500 rounded-2xl flex items-center justify-center text-white text-2xl font-bold shadow-lg">
                    {user?.name ? getInitials(user.name) : '?'}
                  </div>
                )}
              </div>

              {/* Info */}
              <div className="flex-1">
                <h2 className="text-xl font-bold text-gray-900">{user?.name}</h2>
                <p className="text-gray-500">{user?.email}</p>
                
                <div className="flex flex-wrap items-center gap-3 mt-3">
                  <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-indigo-100 text-indigo-700">
                    {user?.role}
                  </span>
                  {user?.organizationName && (
                    <span className="inline-flex items-center gap-1.5 text-sm text-gray-500">
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
                      </svg>
                      {user.organizationName}
                    </span>
                  )}
                </div>

                <p className="text-xs text-gray-400 mt-3">
                  Membro desde {new Date(user?.createdAt || '').toLocaleDateString('pt-BR', { month: 'long', year: 'numeric' })}
                </p>
              </div>
            </div>
          </div>

          {/* Edit Profile */}
          <div className="card p-6 mb-6 animate-fadeIn">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">Informacoes Pessoais</h3>
              {!isEditing && (
                <button
                  onClick={() => setIsEditing(true)}
                  className="btn-secondary text-sm"
                >
                  Editar
                </button>
              )}
            </div>

            {isEditing ? (
              <div className="space-y-4">
                <div>
                  <label className="input-label">Nome</label>
                  <input
                    type="text"
                    value={formData.name}
                    onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                    className="w-full"
                  />
                </div>
                <div>
                  <label className="input-label">URL do Avatar</label>
                  <input
                    type="url"
                    value={formData.avatarUrl}
                    onChange={(e) => setFormData({ ...formData, avatarUrl: e.target.value })}
                    placeholder="https://exemplo.com/avatar.jpg"
                    className="w-full"
                  />
                </div>
                <div className="flex items-center gap-3 pt-2">
                  <button
                    onClick={handleSaveProfile}
                    disabled={isSaving}
                    className="btn-primary"
                  >
                    {isSaving ? 'Salvando...' : 'Salvar'}
                  </button>
                  <button
                    onClick={() => {
                      setIsEditing(false);
                      setFormData({ name: user?.name || '', avatarUrl: user?.avatarUrl || '' });
                    }}
                    className="btn-secondary"
                  >
                    Cancelar
                  </button>
                </div>
              </div>
            ) : (
              <div className="space-y-3">
                <div className="flex items-center justify-between py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-500">Nome</span>
                  <span className="text-sm font-medium text-gray-900">{user?.name}</span>
                </div>
                <div className="flex items-center justify-between py-2 border-b border-gray-100">
                  <span className="text-sm text-gray-500">Email</span>
                  <span className="text-sm font-medium text-gray-900">{user?.email}</span>
                </div>
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-500">Organizacao</span>
                  <span className="text-sm font-medium text-gray-900">{user?.organizationName || '-'}</span>
                </div>
              </div>
            )}
          </div>

          {/* Change Password */}
          <div className="card p-6 animate-fadeIn">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold text-gray-900">Seguranca</h3>
              {!isChangingPassword && (
                <button
                  onClick={() => setIsChangingPassword(true)}
                  className="btn-secondary text-sm"
                >
                  Alterar Senha
                </button>
              )}
            </div>

            {isChangingPassword ? (
              <div className="space-y-4">
                <div>
                  <label className="input-label">Senha Atual</label>
                  <input
                    type="password"
                    value={passwordData.currentPassword}
                    onChange={(e) => setPasswordData({ ...passwordData, currentPassword: e.target.value })}
                    className="w-full"
                  />
                </div>
                <div>
                  <label className="input-label">Nova Senha</label>
                  <input
                    type="password"
                    value={passwordData.newPassword}
                    onChange={(e) => setPasswordData({ ...passwordData, newPassword: e.target.value })}
                    placeholder="Minimo 6 caracteres"
                    className="w-full"
                  />
                </div>
                <div>
                  <label className="input-label">Confirmar Nova Senha</label>
                  <input
                    type="password"
                    value={passwordData.confirmNewPassword}
                    onChange={(e) => setPasswordData({ ...passwordData, confirmNewPassword: e.target.value })}
                    className="w-full"
                  />
                </div>
                <div className="flex items-center gap-3 pt-2">
                  <button
                    onClick={handleChangePassword}
                    disabled={isSaving}
                    className="btn-primary"
                  >
                    {isSaving ? 'Alterando...' : 'Alterar Senha'}
                  </button>
                  <button
                    onClick={() => {
                      setIsChangingPassword(false);
                      setPasswordData({ currentPassword: '', newPassword: '', confirmNewPassword: '' });
                    }}
                    className="btn-secondary"
                  >
                    Cancelar
                  </button>
                </div>
              </div>
            ) : (
              <p className="text-sm text-gray-500">
                Clique em &quot;Alterar Senha&quot; para modificar sua senha de acesso.
              </p>
            )}
          </div>
        </div>
      </div>
    </Layout>
  );
}
