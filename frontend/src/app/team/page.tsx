'use client';

import React, { useEffect, useState } from 'react';
import Layout from '@/components/Layout/Layout';
import { useAuthStore } from '@/stores/authStore';
import {
  authService,
  InviteResponse,
  OrganizationRoleDto,
  CreateOrganizationRoleDto,
} from '@/services/api';

const PERMISSION_LABELS: Record<string, string> = {
  InviteMembers: 'Convidar membros',
  CancelInvites: 'Cancelar convites',
  RemoveMembers: 'Remover membros',
  ManageRoles: 'Gerenciar cargos',
  EditOrganization: 'Editar organização',
};

interface TeamMember {
  id: string;
  name: string;
  email: string;
  avatarUrl?: string;
  role?: string;
}

export default function TeamPage() {
  const { user } = useAuthStore();
  const [members, setMembers] = useState<TeamMember[]>([]);
  const [invites, setInvites] = useState<InviteResponse[]>([]);
  const [customRoles, setCustomRoles] = useState<OrganizationRoleDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [showInviteForm, setShowInviteForm] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteRoleValue, setInviteRoleValue] = useState<string>('Member');
  const [invitePassword, setInvitePassword] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [inviteMessage, setInviteMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [lastInviteLink, setLastInviteLink] = useState<string | null>(null);
  const [lastInviteHasPassword, setLastInviteHasPassword] = useState(false);
  const [showRoleForm, setShowRoleForm] = useState(false);
  const [editingRole, setEditingRole] = useState<OrganizationRoleDto | null>(null);
  const [roleFormName, setRoleFormName] = useState('');
  const [roleFormPermissions, setRoleFormPermissions] = useState<string[]>([]);

  useEffect(() => {
    if (user != null) loadTeamData();
  }, [user]);

  const loadTeamData = async () => {
    try {
      setIsLoading(true);
      setMessage(null);
      const canFetchInvites = user?.canInviteMembers ?? (user?.role === 'Owner' || user?.role === 'Admin');
      const canFetchRoles = user?.canManageRoles ?? false;
      const [membersData, invitesData, rolesData] = await Promise.all([
        authService.getMyOrganizationMembers(),
        canFetchInvites ? authService.getPendingInvites() : Promise.resolve([]),
        canFetchRoles ? authService.getOrganizationRoles() : Promise.resolve([]),
      ]);
      setMembers(membersData.map(m => ({ id: m.userId, name: m.name, email: m.email, avatarUrl: m.avatarUrl, role: m.role })));
      setInvites(Array.isArray(invitesData) ? invitesData : []);
      setCustomRoles(Array.isArray(rolesData) ? rolesData : []);
    } catch (error: any) {
      console.error('Error loading team data:', error);
      const data = error.response?.data;
      const apiMessage = data?.error ?? data?.message;
      const status = error.response?.status;
      const text =
        apiMessage
        ?? (status === 401 ? 'Sessao invalida. Faca login novamente.' : undefined)
        ?? (status === 403 ? 'Voce nao tem permissao para ver esta equipe.' : undefined)
        ?? 'Erro ao carregar a equipe.';
      setMessage({ type: 'error', text });
    } finally {
      setIsLoading(false);
    }
  };

  const handleSendInvite = async () => {
    if (!inviteEmail) {
      setInviteMessage({ type: 'error', text: 'Digite o email do convidado' });
      return;
    }

    setIsSending(true);
    setInviteMessage(null);
    setLastInviteLink(null);

    try {
      const isCustomRole = /^[0-9a-f-]{36}$/i.test(inviteRoleValue);
      const created = await authService.inviteUser({
        email: inviteEmail,
        role: isCustomRole ? 'Member' : inviteRoleValue,
        ...(isCustomRole ? { customRoleId: inviteRoleValue } : {}),
        ...(invitePassword.trim() ? { invitePassword: invitePassword.trim() } : {}),
      });
      const code = created?.inviteCode ?? (created as any)?.InviteCode;
      setInviteMessage({ type: 'success', text: 'Convite criado com sucesso! Compartilhe o link abaixo com o convidado.' });
      setInviteEmail('');
      const hadPassword = !!invitePassword.trim();
      setInvitePassword('');
      if (typeof window !== 'undefined' && code) {
        setLastInviteLink(`${window.location.origin}/register?invite=${encodeURIComponent(code)}`);
        setLastInviteHasPassword(hadPassword);
      }
      loadTeamData();
    } catch (error: any) {
      const data = error.response?.data;
      const apiMessage = data?.error ?? data?.message;
      const status = error.response?.status;
      const text =
        apiMessage
        ?? (status === 401 ? 'Sessao invalida. Faca login novamente.' : undefined)
        ?? (status === 403 ? 'Voce nao tem permissao para convidar nesta organizacao.' : undefined)
        ?? 'Erro ao enviar convite.';
      setInviteMessage({ type: 'error', text });
    } finally {
      setIsSending(false);
    }
  };

  const copyInviteLink = () => {
    if (!lastInviteLink) return;
    navigator.clipboard.writeText(lastInviteLink).then(() => {
      setInviteMessage(prev =>
        prev && prev.type === 'success'
          ? { type: 'success', text: 'Link copiado para a area de transferencia!' }
          : prev
      );
    });
  };

  const handleCancelInvite = async (inviteId: string) => {
    if (!confirm('Tem certeza que deseja cancelar este convite?')) return;

    try {
      await authService.cancelInvite(inviteId);
      loadTeamData();
    } catch (error) {
      console.error('Error canceling invite:', error);
    }
  };

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  const canManageTeam = user?.canInviteMembers ?? (user?.role === 'Owner' || user?.role === 'Admin');
  const canManageRoles = user?.canManageRoles ?? false;
  const canRemoveMembers = user?.canRemoveMembers ?? false;
  const hasOrg = !!user?.organizationId;

  const openNewRoleForm = () => {
    setEditingRole(null);
    setRoleFormName('');
    setRoleFormPermissions([]);
    setShowRoleForm(true);
  };

  const openEditRoleForm = (role: OrganizationRoleDto) => {
    setEditingRole(role);
    setRoleFormName(role.name);
    setRoleFormPermissions(role.permissions ?? []);
    setShowRoleForm(true);
  };

  const handleSaveRole = async () => {
    if (!roleFormName.trim()) {
      setMessage({ type: 'error', text: 'Nome do cargo e obrigatorio.' });
      return;
    }
    setMessage(null);
    try {
      if (editingRole) {
        await authService.updateOrganizationRole(editingRole.id, { name: roleFormName.trim(), permissions: roleFormPermissions });
        setMessage({ type: 'success', text: 'Cargo atualizado.' });
      } else {
        await authService.createOrganizationRole({ name: roleFormName.trim(), permissions: roleFormPermissions });
        setMessage({ type: 'success', text: 'Cargo criado.' });
      }
      setShowRoleForm(false);
      loadTeamData();
    } catch (err: any) {
      const data = err.response?.data;
      setMessage({ type: 'error', text: data?.error ?? data?.message ?? 'Erro ao salvar cargo.' });
    }
  };

  const handleDeleteRole = async (role: OrganizationRoleDto) => {
    if (!confirm(`Excluir o cargo "${role.name}"? Nao sera possivel se algum membro estiver usando.`)) return;
    try {
      await authService.deleteOrganizationRole(role.id);
      setMessage({ type: 'success', text: 'Cargo excluido.' });
      loadTeamData();
    } catch (err: any) {
      const data = err.response?.data;
      setMessage({ type: 'error', text: data?.error ?? 'Erro ao excluir cargo.' });
    }
  };

  const toggleRolePermission = (perm: string) => {
    setRoleFormPermissions(prev =>
      prev.includes(perm) ? prev.filter(p => p !== perm) : [...prev, perm]
    );
  };

  const handleRemoveMember = async (memberUserId: string, memberRole: string) => {
    if (memberRole === 'Owner') return;
    if (!confirm('Remover este membro da organizacao?')) return;
    try {
      await authService.removeOrganizationMember(memberUserId);
      setMessage({ type: 'success', text: 'Membro removido.' });
      loadTeamData();
    } catch (err: any) {
      const data = err.response?.data;
      setMessage({ type: 'error', text: data?.error ?? 'Erro ao remover membro.' });
    }
  };

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white dark:from-slate-900 dark:to-slate-800 py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-4xl mx-auto">
          {/* Header */}
          <div className="flex items-start justify-between mb-8">
            <div>
              <h1 className="page-title">Equipe</h1>
              <p className="text-muted mt-1">
                {canManageTeam ? 'Gerencie os membros da sua organizacao' : 'Membros e convites pendentes da sua organizacao (somente visualizacao)'}
              </p>
            </div>
            {canManageTeam && (
              <button
                onClick={() => setShowInviteForm(true)}
                className="btn-primary"
              >
                <svg className="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                Convidar
              </button>
            )}
          </div>

          {!hasOrg && !isLoading && (
            <div className="mb-6 p-4 rounded-xl border bg-amber-50 border-amber-200 text-amber-800">
              <p className="text-sm font-medium">Voce nao pertence a nenhuma organizacao. Aceite um convite em Notificacoes para entrar em uma equipe.</p>
            </div>
          )}

          {/* Cargos da organização (Owner ou ManageRoles) */}
          {canManageRoles && !isLoading && hasOrg && (
            <div className="card mb-6 animate-fadeIn">
              <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
                <h2 className="font-semibold text-gray-900">Cargos da organizacao</h2>
                <button type="button" onClick={openNewRoleForm} className="btn-secondary text-sm">
                  Novo cargo
                </button>
              </div>
              <div className="divide-y divide-gray-100">
                {customRoles.length === 0 ? (
                  <div className="p-6 text-sm text-gray-500">Nenhum cargo customizado. Crie um para atribuir permissoes especificas.</div>
                ) : (
                  customRoles.map((role) => (
                    <div key={role.id} className="p-4 flex items-center justify-between gap-4">
                      <div>
                        <p className="font-medium text-gray-900">{role.name}</p>
                        <div className="flex flex-wrap gap-1 mt-1">
                          {(role.permissions ?? []).map((p) => (
                            <span key={p} className="px-2 py-0.5 rounded text-xs bg-gray-100 text-gray-700">
                              {PERMISSION_LABELS[p] ?? p}
                            </span>
                          ))}
                        </div>
                      </div>
                      <div className="flex items-center gap-2">
                        <button type="button" onClick={() => openEditRoleForm(role)} className="text-sm text-indigo-600 hover:text-indigo-700 font-medium">
                          Editar
                        </button>
                        <button type="button" onClick={() => handleDeleteRole(role)} className="text-sm text-red-600 hover:text-red-700 font-medium">
                          Excluir
                        </button>
                      </div>
                    </div>
                  ))
                )}
              </div>
            </div>
          )}

          {/* Message */}
          {message && (
            <div className={`mb-6 p-4 rounded-xl border animate-fadeIn ${
              message.type === 'success' 
                ? 'bg-emerald-50 border-emerald-200 text-emerald-700' 
                : 'bg-red-50 border-red-200 text-red-700'
            }`}>
              <div className="flex items-center gap-2">
                {message.type === 'success' ? (
                  <svg className="w-5 h-5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                ) : (
                  <svg className="w-5 h-5 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                  </svg>
                )}
                <span className="text-sm font-medium">{message.text}</span>
              </div>
            </div>
          )}

          {/* Invite Form Modal (apenas Owner/Admin) */}
          {canManageTeam && showInviteForm && (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
              <div className="card p-6 w-full max-w-md animate-scaleIn">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">Convidar Membro</h2>

                {inviteMessage && (
                  <div
                    className={`mb-4 p-3 rounded-lg border text-sm ${
                      inviteMessage.type === 'success'
                        ? 'bg-emerald-50 border-emerald-200 text-emerald-700'
                        : 'bg-red-50 border-red-200 text-red-700'
                    }`}
                  >
                    {inviteMessage.text}
                  </div>
                )}
                
                <div className="space-y-4">
                  <div>
                    <label className="input-label">Email</label>
                    <input
                      type="email"
                      value={inviteEmail}
                      onChange={(e) => setInviteEmail(e.target.value)}
                      placeholder="email@exemplo.com"
                      className="w-full"
                    />
                  </div>

                  <div>
                    <label className="input-label">Cargo</label>
                    <select
                      value={inviteRoleValue}
                      onChange={(e) => setInviteRoleValue(e.target.value)}
                      className="w-full"
                    >
                      <option value="Member">Membro</option>
                      <option value="Admin">Administrador</option>
                      {customRoles.map((r) => (
                        <option key={r.id} value={r.id}>{r.name}</option>
                      ))}
                    </select>
                  </div>

                  <div>
                    <label className="input-label">Senha do convite (opcional)</label>
                    <input
                      type="password"
                      value={invitePassword}
                      onChange={(e) => setInvitePassword(e.target.value)}
                      placeholder="Se definida, o convidado precisara informa-la ao se cadastrar"
                      className="w-full"
                      autoComplete="new-password"
                    />
                    <p className="text-xs text-gray-500 mt-1">Deixe em branco para convite sem senha</p>
                  </div>
                </div>

                {lastInviteLink && (
                  <div className="mt-4 pt-3 border-t border-gray-100">
                    <p className="text-xs font-medium text-gray-700 mb-1">
                      Link do convite (compartilhe com o convidado):
                    </p>
                    <div className="flex gap-2">
                      <input
                        type="text"
                        readOnly
                        value={lastInviteLink}
                        className="flex-1 rounded-lg border border-gray-200 bg-white px-3 py-2 text-sm text-gray-700"
                      />
                      <button
                        type="button"
                        onClick={copyInviteLink}
                        className="btn-secondary text-sm py-2 px-3 whitespace-nowrap"
                      >
                        Copiar link
                      </button>
                    </div>
                    {lastInviteHasPassword && (
                      <p className="text-xs text-gray-500 mt-1">
                        O convidado precisara da senha do convite ao se cadastrar.
                      </p>
                    )}
                    <button
                      type="button"
                      onClick={() => setLastInviteLink(null)}
                      className="text-xs text-gray-500 hover:text-gray-700 mt-2"
                    >
                      Fechar
                    </button>
                  </div>
                )}

                <div className="flex items-center gap-3 mt-6">
                  <button
                    onClick={handleSendInvite}
                    disabled={isSending}
                    className="btn-primary flex-1"
                  >
                    {isSending ? 'Enviando...' : 'Enviar Convite'}
                  </button>
                  <button
                    onClick={() => {
                      setShowInviteForm(false);
                      setInviteEmail('');
                      setInviteRoleValue('Member');
                      setInvitePassword('');
                      setInviteMessage(null);
                      setLastInviteLink(null);
                    }}
                    className="btn-secondary"
                  >
                    Cancelar
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Role form modal (create / edit) */}
          {showRoleForm && (
            <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
              <div className="card p-6 w-full max-w-md animate-scaleIn">
                <h2 className="text-lg font-semibold text-gray-900 mb-4">
                  {editingRole ? 'Editar cargo' : 'Novo cargo'}
                </h2>
                <div className="space-y-4">
                  <div>
                    <label className="input-label">Nome do cargo</label>
                    <input
                      type="text"
                      value={roleFormName}
                      onChange={(e) => setRoleFormName(e.target.value)}
                      placeholder="Ex.: Gerente de projetos"
                      className="w-full"
                    />
                  </div>
                  <div>
                    <label className="input-label">Permissoes</label>
                    <div className="space-y-2 mt-1">
                      {Object.entries(PERMISSION_LABELS).map(([key, label]) => (
                        <label key={key} className="flex items-center gap-2 cursor-pointer">
                          <input
                            type="checkbox"
                            checked={roleFormPermissions.includes(key)}
                            onChange={() => toggleRolePermission(key)}
                            className="rounded border-gray-300"
                          />
                          <span className="text-sm text-gray-700">{label}</span>
                        </label>
                      ))}
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-3 mt-6">
                  <button type="button" onClick={handleSaveRole} className="btn-primary flex-1">
                    {editingRole ? 'Salvar' : 'Criar'}
                  </button>
                  <button
                    type="button"
                    onClick={() => setShowRoleForm(false)}
                    className="btn-secondary"
                  >
                    Cancelar
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Loading */}
          {isLoading && (
            <div className="card divide-y divide-gray-100">
              {[1, 2, 3].map((i) => (
                <div key={i} className="p-4 flex items-center gap-4">
                  <div className="w-10 h-10 skeleton rounded-full"></div>
                  <div className="flex-1">
                    <div className="h-4 skeleton w-1/3 mb-2"></div>
                    <div className="h-3 skeleton w-1/4"></div>
                  </div>
                </div>
              ))}
            </div>
          )}

          {/* Members */}
          {!isLoading && (
            <>
              <div className="card mb-6 animate-fadeIn">
                <div className="px-6 py-4 border-b border-gray-100">
                  <h2 className="font-semibold text-gray-900">Membros ({members.length})</h2>
                </div>
                <div className="divide-y divide-gray-100">
                  {members.map((member) => (
                    <div key={member.id} className="p-4 flex items-center gap-4 hover:bg-gray-50">
                      {member.avatarUrl ? (
                        <img
                          src={member.avatarUrl}
                          alt={member.name}
                          className="w-10 h-10 rounded-full object-cover"
                        />
                      ) : (
                        <div className="w-10 h-10 bg-gradient-to-br from-indigo-500 to-violet-500 rounded-full flex items-center justify-center text-white text-sm font-medium">
                          {getInitials(member.name)}
                        </div>
                      )}
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <span className="font-medium text-gray-900">{member.name}</span>
                          {member.id === user?.id && (
                            <span className="text-xs text-gray-400">(voce)</span>
                          )}
                        </div>
                        <span className="text-sm text-gray-500">{member.email}</span>
                      </div>
                      {member.role && (
                        <span className={`px-2.5 py-1 rounded-full text-xs font-medium ${
                          member.role === 'Owner'
                            ? 'bg-purple-100 text-purple-700'
                            : member.role === 'Admin'
                            ? 'bg-blue-100 text-blue-700'
                            : 'bg-gray-100 text-gray-700'
                        }`}>
                          {member.role}
                        </span>
                      )}
                      {canRemoveMembers && member.role !== 'Owner' && member.id !== user?.id && (
                        <button
                          type="button"
                          onClick={() => handleRemoveMember(member.id, member.role ?? '')}
                          className="text-sm text-red-600 hover:text-red-700 font-medium"
                        >
                          Remover
                        </button>
                      )}
                    </div>
                  ))}
                </div>
              </div>

              {/* Pending Invites */}
              {invites.length > 0 && (
                <div className="card animate-fadeIn">
                  <div className="px-6 py-4 border-b border-gray-100">
                    <h2 className="font-semibold text-gray-900">Convites Pendentes ({invites.length})</h2>
                  </div>
                  <div className="divide-y divide-gray-100">
                    {invites.map((invite) => {
                      const code = invite.inviteCode ?? (invite as any).InviteCode ?? '';
                      const inviteLink = typeof window !== 'undefined' && code
                        ? `${window.location.origin}/register?invite=${encodeURIComponent(code)}`
                        : '';
                      const copyInviteLink = () => {
                        if (!inviteLink) return;
                        navigator.clipboard.writeText(inviteLink).then(() => {
                          setMessage(prev => prev?.type === 'success' ? { type: 'success', text: 'Link copiado!' } : { type: 'success', text: 'Link copiado para a area de transferencia.' });
                        });
                      };
                      return (
                      <div key={invite.id} className="p-4 flex items-center gap-4 hover:bg-gray-50">
                        <div className="w-10 h-10 bg-gray-100 rounded-full flex items-center justify-center">
                          <svg className="w-5 h-5 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                          </svg>
                        </div>
                        <div className="flex-1 min-w-0">
                          <span className="font-medium text-gray-900">{invite.email}</span>
                          <div className="flex items-center gap-2 mt-0.5 flex-wrap">
                            <span className="text-xs text-gray-500">
                              Expira em {new Date(invite.expiresAt ?? (invite as any).ExpiresAt).toLocaleDateString('pt-BR')}
                            </span>
                            <span className="px-2 py-0.5 rounded-full text-xs bg-amber-100 text-amber-700">
                              Pendente
                            </span>
                            {(invite.hasPassword ?? (invite as any).HasPassword) && (
                              <span className="px-2 py-0.5 rounded-full text-xs bg-slate-100 text-slate-600">
                                Protegido por senha
                              </span>
                            )}
                          </div>
                          {canManageTeam && inviteLink && (
                            <p className="text-xs text-gray-500 mt-1 truncate" title={inviteLink}>
                              Link: {inviteLink}
                            </p>
                          )}
                        </div>
                        {canManageTeam && (
                          <div className="flex items-center gap-2 flex-shrink-0">
                            {inviteLink && (
                              <button
                                type="button"
                                onClick={copyInviteLink}
                                className="text-sm text-indigo-600 hover:text-indigo-700 font-medium"
                              >
                                Copiar link
                              </button>
                            )}
                            <button
                              onClick={() => handleCancelInvite(invite.id)}
                              className="text-sm text-red-600 hover:text-red-700 font-medium"
                            >
                              Cancelar
                            </button>
                          </div>
                        )}
                      </div>
                    ); })}
                  </div>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </Layout>
  );
}
