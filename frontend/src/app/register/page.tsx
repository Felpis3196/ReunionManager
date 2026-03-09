'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import { useAuthStore } from '@/stores/authStore';

export default function RegisterPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const inviteCode = searchParams.get('invite');

  const { register, isLoading, error, isAuthenticated, clearError } = useAuthStore();

  const [formData, setFormData] = useState({
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
    organizationName: '',
    inviteCode: inviteCode || '',
    invitePassword: '',
    acceptTerms: false,
  });
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [showPassword, setShowPassword] = useState(false);

  useEffect(() => {
    if (isAuthenticated) router.push('/');
  }, [isAuthenticated, router]);

  useEffect(() => {
    clearError();
  }, [clearError]);

  // Sync invite code from URL so the link /register?invite=CODE works (searchParams may be available only after hydration)
  useEffect(() => {
    const fromUrl = searchParams.get('invite');
    if (fromUrl && fromUrl !== formData.inviteCode) {
      setFormData((prev) => ({ ...prev, inviteCode: fromUrl }));
    }
  }, [searchParams]);

  const validateForm = () => {
    const errors: Record<string, string> = {};
    if (!formData.name || formData.name.length < 2) {
      errors.name = 'Nome deve ter no minimo 2 caracteres';
    }
    if (!formData.email) {
      errors.email = 'Email e obrigatorio';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
      errors.email = 'Email invalido';
    }
    if (!formData.password) {
      errors.password = 'Senha e obrigatoria';
    } else if (formData.password.length < 6) {
      errors.password = 'Senha deve ter no minimo 6 caracteres';
    }
    if (formData.password !== formData.confirmPassword) {
      errors.confirmPassword = 'Senhas nao conferem';
    }
    if (!formData.acceptTerms) {
      errors.acceptTerms = 'Aceite os termos para continuar';
    }
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) return;
    const success = await register({
      name: formData.name,
      email: formData.email,
      password: formData.password,
      confirmPassword: formData.confirmPassword,
      organizationName: formData.organizationName || undefined,
      inviteCode: formData.inviteCode || undefined,
      invitePassword: formData.invitePassword || undefined,
    });
    if (success) router.push('/');
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="absolute inset-0 overflow-hidden pointer-events-none">
        <div className="absolute -top-40 -right-40 w-80 h-80 bg-indigo-100 rounded-full blur-3xl opacity-40" />
        <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-violet-100 rounded-full blur-3xl opacity-40" />
      </div>

      <div className="relative w-full max-w-md">
        <div className="bg-white rounded-2xl shadow-xl border border-gray-100 p-8 sm:p-10">
          <div className="text-center mb-8">
            <div className="w-12 h-12 bg-indigo-600 rounded-xl flex items-center justify-center mx-auto mb-4">
              <svg className="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
              </svg>
            </div>
            <h1 className="text-xl font-bold text-gray-900">Criar conta</h1>
            <p className="text-gray-500 mt-1 text-sm">
              {inviteCode ? 'Voce foi convidado para uma organizacao' : 'Preencha os dados abaixo'}
            </p>
          </div>

          {error && (
            <div className="mb-6 p-4 bg-red-50 border border-red-100 rounded-xl">
              <div className="flex items-start gap-3">
                <svg className="w-5 h-5 text-red-500 flex-shrink-0 mt-0.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                <div className="min-w-0 flex-1">
                  <p className="text-sm text-red-700 whitespace-pre-wrap break-words">{error}</p>
                  {(error.includes('Network') || error.includes('timeout') || error.includes('ERR_')) && (
                    <p className="text-xs text-red-600 mt-2">Verifique se a API esta rodando (ex: http://localhost:5000).</p>
                  )}
                </div>
              </div>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">Nome completo</label>
              <input
                id="name"
                name="name"
                type="text"
                autoComplete="name"
                className={`block w-full rounded-lg border px-4 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent ${formErrors.name ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-white'}`}
                placeholder="Seu nome"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              />
              {formErrors.name && <p className="mt-1 text-sm text-red-600">{formErrors.name}</p>}
            </div>

            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">Email</label>
              <input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                className={`block w-full rounded-lg border px-4 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent ${formErrors.email ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-white'}`}
                placeholder="seu@email.com"
                value={formData.email}
                onChange={(e) => setFormData({ ...formData, email: e.target.value })}
              />
              {formErrors.email && <p className="mt-1 text-sm text-red-600">{formErrors.email}</p>}
            </div>

            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">Senha (min. 6 caracteres)</label>
              <div className="relative">
                <input
                  id="password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="new-password"
                  className={`block w-full rounded-lg border px-4 py-2.5 pr-10 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent ${formErrors.password ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-white'}`}
                  placeholder="Sua senha"
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                />
                <button
                  type="button"
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  onClick={() => setShowPassword(!showPassword)}
                >
                  {showPassword ? (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                    </svg>
                  ) : (
                    <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                  )}
                </button>
              </div>
              {formErrors.password && <p className="mt-1 text-sm text-red-600">{formErrors.password}</p>}
            </div>

            <div>
              <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700 mb-1">Confirmar senha</label>
              <input
                id="confirmPassword"
                name="confirmPassword"
                type="password"
                autoComplete="new-password"
                className={`block w-full rounded-lg border px-4 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent ${formErrors.confirmPassword ? 'border-red-300 bg-red-50' : 'border-gray-200 bg-white'}`}
                placeholder="Repita a senha"
                value={formData.confirmPassword}
                onChange={(e) => setFormData({ ...formData, confirmPassword: e.target.value })}
              />
              {formErrors.confirmPassword && <p className="mt-1 text-sm text-red-600">{formErrors.confirmPassword}</p>}
            </div>

            {!inviteCode && (
              <div>
                <label htmlFor="organizationName" className="block text-sm font-medium text-gray-700 mb-1">
                  Nome da organizacao <span className="text-gray-400 font-normal">(opcional)</span>
                </label>
                <input
                  id="organizationName"
                  name="organizationName"
                  type="text"
                  className="block w-full rounded-lg border border-gray-200 bg-white px-4 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                  placeholder="Sua empresa ou equipe"
                  value={formData.organizationName}
                  onChange={(e) => setFormData({ ...formData, organizationName: e.target.value })}
                />
                <p className="text-xs text-gray-500 mt-1">
                  Sem organizacao voce podera entrar em uma equipe depois, aceitando um convite em Notificacoes.
                </p>
              </div>
            )}

            {inviteCode && (
              <>
                <div className="p-3 bg-indigo-50 border border-indigo-100 rounded-xl">
                  <p className="text-sm text-indigo-700">Codigo de convite: {inviteCode}</p>
                </div>
                <div>
                  <label htmlFor="invitePassword" className="block text-sm font-medium text-gray-700 mb-1">
                    Senha do convite <span className="text-gray-400 font-normal">(se o convite tiver senha)</span>
                  </label>
                  <input
                    id="invitePassword"
                    name="invitePassword"
                    type="password"
                    autoComplete="off"
                    className="block w-full rounded-lg border border-gray-200 bg-white px-4 py-2.5 text-sm text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                    placeholder="Informe a senha que recebeu com o convite"
                    value={formData.invitePassword}
                    onChange={(e) => setFormData({ ...formData, invitePassword: e.target.value })}
                  />
                </div>
              </>
            )}

            <div className="flex items-start gap-2">
              <input
                id="acceptTerms"
                name="acceptTerms"
                type="checkbox"
                className="mt-1 w-4 h-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"
                checked={formData.acceptTerms}
                onChange={(e) => setFormData({ ...formData, acceptTerms: e.target.checked })}
              />
              <label htmlFor="acceptTerms" className="text-sm text-gray-600">
                Eu concordo com os termos de uso e politica de privacidade
              </label>
            </div>
            {formErrors.acceptTerms && <p className="text-sm text-red-600">{formErrors.acceptTerms}</p>}

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-3 px-4 rounded-lg bg-indigo-600 text-white font-medium text-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
            >
              {isLoading ? (
                <span className="flex items-center justify-center gap-2">
                  <svg className="animate-spin h-5 w-5 text-white" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                  </svg>
                  Criando conta...
                </span>
              ) : (
                'Criar conta'
              )}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-gray-600">
            Ja tem conta?{' '}
            <Link href="/login" className="font-medium text-indigo-600 hover:text-indigo-700">
              Entrar
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}
