'use client';

import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useRouter } from 'next/navigation';
import Layout from '@/components/Layout/Layout';
import Link from 'next/link';
import { Meeting, MeetingStatus, MeetingType, ParticipantStatus } from '@/types/meeting';
import { meetingService, agendaService, decisionService, taskService, AgendaItem, Decision, Task, userService } from '@/services/api';

const statusConfig: Record<MeetingStatus, { label: string; color: string; bg: string; dot: string }> = {
  [MeetingStatus.Scheduled]: { label: 'Agendada', color: 'text-indigo-700', bg: 'bg-indigo-50', dot: 'bg-indigo-500' },
  [MeetingStatus.InProgress]: { label: 'Em Andamento', color: 'text-emerald-700', bg: 'bg-emerald-50', dot: 'bg-emerald-500' },
  [MeetingStatus.Completed]: { label: 'Concluida', color: 'text-gray-600', bg: 'bg-gray-100', dot: 'bg-gray-400' },
  [MeetingStatus.Cancelled]: { label: 'Cancelada', color: 'text-red-700', bg: 'bg-red-50', dot: 'bg-red-500' },
};

const typeLabels: Record<MeetingType, string> = {
  [MeetingType.Planning]: 'Planejamento',
  [MeetingType.Review]: 'Revisao',
  [MeetingType.Standup]: 'Daily Standup',
  [MeetingType.Retrospective]: 'Retrospectiva',
  [MeetingType.OneOnOne]: '1:1',
  [MeetingType.Other]: 'Outro',
};

const participantStatusConfig: Record<ParticipantStatus, { label: string; color: string }> = {
  [ParticipantStatus.Invited]: { label: 'Convidado', color: 'bg-amber-50 text-amber-700' },
  [ParticipantStatus.Accepted]: { label: 'Confirmado', color: 'bg-emerald-50 text-emerald-700' },
  [ParticipantStatus.Declined]: { label: 'Recusou', color: 'bg-red-50 text-red-700' },
  [ParticipantStatus.Attended]: { label: 'Presente', color: 'bg-blue-50 text-blue-700' },
  [ParticipantStatus.Absent]: { label: 'Ausente', color: 'bg-gray-100 text-gray-600' },
};

export default function MeetingDetailPage() {
  const params = useParams();
  const router = useRouter();
  const meetingId = params.id as string;

  const [meeting, setMeeting] = useState<Meeting | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'details' | 'agenda' | 'decisions' | 'tasks'>('details');

  const [isEditing, setIsEditing] = useState(false);
  const [editForm, setEditForm] = useState({ title: '', description: '', location: '', meetingUrl: '', scheduledAt: '', duration: '' });
  const [isSaving, setIsSaving] = useState(false);

  const [agendaItems, setAgendaItems] = useState<AgendaItem[]>([]);
  const [newAgendaItem, setNewAgendaItem] = useState({ title: '', description: '', estimatedMinutes: 15 });
  const [showAgendaForm, setShowAgendaForm] = useState(false);

  const [decisions, setDecisions] = useState<Decision[]>([]);
  const [newDecision, setNewDecision] = useState({ title: '', description: '' });
  const [showDecisionForm, setShowDecisionForm] = useState(false);

  const [tasks, setTasks] = useState<Task[]>([]);
  const [newTask, setNewTask] = useState({ title: '', description: '', priority: 'Medium', dueDate: '' });
  const [showTaskForm, setShowTaskForm] = useState(false);
  const [users, setUsers] = useState<{ id: string; name: string; email: string }[]>([]);
  const [selectedUserId, setSelectedUserId] = useState('');

  const loadAllData = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);
      const [meetingData, agendaData, decisionsData, tasksData, usersData] = await Promise.all([
        meetingService.getById(meetingId),
        agendaService.getAll(meetingId).catch(() => []),
        decisionService.getAll(meetingId).catch(() => []),
        taskService.getAll({ meetingId }).catch(() => []),
        userService.getAll().catch(() => []),
      ]);
      setMeeting(meetingData);
      setAgendaItems(agendaData);
      setDecisions(decisionsData);
      setTasks(tasksData);
      setUsers(usersData);
      setEditForm({
        title: meetingData.title,
        description: meetingData.description || '',
        location: meetingData.location || '',
        meetingUrl: meetingData.meetingUrl || '',
        scheduledAt: meetingData.scheduledAt?.slice(0, 16) || '',
        duration: meetingData.duration || '01:00',
      });
      if (usersData.length > 0) setSelectedUserId(usersData[0].id);
    } catch (err: any) {
      setError(err.response?.data?.error || err.message || 'Erro ao carregar dados');
    } finally {
      setIsLoading(false);
    }
  }, [meetingId]);

  useEffect(() => {
    if (meetingId) loadAllData();
  }, [meetingId, loadAllData]);

  const handleSave = async () => {
    if (!meeting) return;
    try {
      setIsSaving(true);
      const updated = await meetingService.update(meeting.id, editForm);
      setMeeting(updated);
      setIsEditing(false);
    } catch (err: any) {
      alert(err.response?.data?.message || 'Erro ao atualizar');
    } finally {
      setIsSaving(false);
    }
  };

  const handleStart = async () => {
    if (!meeting) return;
    try {
      const updated = await meetingService.startMeeting(meeting.id);
      setMeeting(updated);
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro ao iniciar');
    }
  };

  const handleEnd = async () => {
    if (!meeting) return;
    try {
      const updated = await meetingService.endMeeting(meeting.id);
      setMeeting(updated);
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro ao finalizar');
    }
  };

  const handleCancel = async () => {
    if (!meeting || !confirm('Cancelar esta reuniao?')) return;
    try {
      const updated = await meetingService.cancelMeeting(meeting.id);
      setMeeting(updated);
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro ao cancelar');
    }
  };

  const handleDelete = async () => {
    if (!meeting || !confirm('Excluir esta reuniao permanentemente?')) return;
    try {
      await meetingService.delete(meeting.id);
      router.push('/');
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro ao excluir');
    }
  };

  // Agenda handlers
  const handleAddAgendaItem = async () => {
    if (!newAgendaItem.title.trim()) return;
    try {
      await agendaService.create(meetingId, newAgendaItem);
      setNewAgendaItem({ title: '', description: '', estimatedMinutes: 15 });
      setShowAgendaForm(false);
      setAgendaItems(await agendaService.getAll(meetingId));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro ao adicionar item');
    }
  };

  const handleToggleAgendaItem = async (item: AgendaItem) => {
    try {
      if (item.isCompleted) await agendaService.update(meetingId, item.id, { isCompleted: false });
      else await agendaService.markAsComplete(meetingId, item.id);
      setAgendaItems(await agendaService.getAll(meetingId));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro');
    }
  };

  const handleDeleteAgendaItem = async (item: AgendaItem) => {
    try {
      await agendaService.delete(meetingId, item.id);
      setAgendaItems(agendaItems.filter(a => a.id !== item.id));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro');
    }
  };

  // Decision handlers
  const handleAddDecision = async () => {
    if (!newDecision.title.trim()) return;
    try {
      await decisionService.create(meetingId, newDecision);
      setNewDecision({ title: '', description: '' });
      setShowDecisionForm(false);
      setDecisions(await decisionService.getAll(meetingId));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro');
    }
  };

  const handleToggleDecision = async (decision: Decision) => {
    try {
      if (decision.isImplemented) await decisionService.update(meetingId, decision.id, { isImplemented: false });
      else await decisionService.markAsImplemented(meetingId, decision.id);
      setDecisions(await decisionService.getAll(meetingId));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro');
    }
  };

  const handleDeleteDecision = async (decision: Decision) => {
    try {
      await decisionService.delete(meetingId, decision.id);
      setDecisions(decisions.filter(d => d.id !== decision.id));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro');
    }
  };

  // Task handlers
  const handleAddTask = async () => {
    if (!newTask.title.trim() || !selectedUserId) return;
    try {
      await taskService.create({ meetingId, assignedToId: selectedUserId, title: newTask.title, description: newTask.description || undefined, priority: newTask.priority, dueDate: newTask.dueDate || undefined });
      setNewTask({ title: '', description: '', priority: 'Medium', dueDate: '' });
      setShowTaskForm(false);
      setTasks(await taskService.getAll({ meetingId }));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro');
    }
  };

  const handleCompleteTask = async (task: Task) => {
    try {
      await taskService.complete(task.id);
      setTasks(await taskService.getAll({ meetingId }));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro');
    }
  };

  const handleDeleteTask = async (task: Task) => {
    try {
      await taskService.delete(task.id);
      setTasks(tasks.filter(t => t.id !== task.id));
    } catch (err: any) {
      alert(err.response?.data?.error || 'Erro');
    }
  };

  const formatDateTime = (dateStr: string) => new Date(dateStr).toLocaleString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });

  const formatDuration = (duration: string) => {
    if (!duration) return '';
    const [h, m] = duration.split(':').map(Number);
    if (h > 0 && m > 0) return `${h}h ${m}min`;
    if (h > 0) return `${h}h`;
    return `${m}min`;
  };

  if (isLoading) {
    return (
      <Layout>
        <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white py-8 px-4">
          <div className="max-w-5xl mx-auto">
            <div className="h-4 w-20 skeleton mb-4"></div>
            <div className="card p-6">
              <div className="h-8 skeleton w-1/2 mb-4"></div>
              <div className="h-4 skeleton w-1/4 mb-6"></div>
              <div className="grid grid-cols-2 gap-4">
                <div className="h-20 skeleton rounded-xl"></div>
                <div className="h-20 skeleton rounded-xl"></div>
              </div>
            </div>
          </div>
        </div>
      </Layout>
    );
  }

  if (error || !meeting) {
    return (
      <Layout>
        <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white py-8 px-4">
          <div className="max-w-3xl mx-auto">
            <div className="card border-red-100 bg-red-50 p-8 text-center">
              <div className="w-12 h-12 mx-auto bg-red-100 rounded-xl flex items-center justify-center mb-4">
                <AlertIcon className="w-6 h-6 text-red-600" />
              </div>
              <h3 className="text-lg font-semibold text-red-800">{error || 'Reuniao nao encontrada'}</h3>
              <Link href="/" className="btn-primary mt-4 inline-flex">Voltar</Link>
            </div>
          </div>
        </div>
      </Layout>
    );
  }

  const status = statusConfig[meeting.status];

  return (
    <Layout>
      <div className="min-h-screen bg-gradient-to-b from-slate-50 to-white py-8 px-4 sm:px-6 lg:px-8">
        <div className="max-w-5xl mx-auto">
          {/* Back */}
          <Link href="/" className="inline-flex items-center text-sm text-gray-500 hover:text-gray-700 mb-4 group">
            <ChevronLeftIcon className="w-4 h-4 mr-1 transition-transform group-hover:-translate-x-1" />
            Voltar
          </Link>

          {/* Header Card */}
          <div className="card mb-6 animate-fadeIn">
            <div className="p-6">
              <div className="flex flex-col lg:flex-row lg:items-start justify-between gap-4">
                <div className="flex-1 min-w-0">
                  {isEditing ? (
                    <input type="text" value={editForm.title} onChange={(e) => setEditForm({ ...editForm, title: e.target.value })} className="text-2xl font-bold w-full" />
                  ) : (
                    <h1 className="page-title truncate">{meeting.title}</h1>
                  )}
                  <div className="mt-3 flex flex-wrap items-center gap-2">
                    <span className={`badge ${status.bg} ${status.color}`}>
                      <span className={`w-1.5 h-1.5 rounded-full ${status.dot} mr-1.5`}></span>
                      {status.label}
                    </span>
                    <span className="badge badge-primary">{typeLabels[meeting.type]}</span>
                    <span className="text-sm text-gray-500 flex items-center gap-1">
                      <ClockIcon className="w-4 h-4" />
                      {formatDateTime(meeting.scheduledAt)} - {formatDuration(meeting.duration)}
                    </span>
                  </div>
                </div>

                {/* Actions */}
                <div className="flex flex-wrap gap-2">
                  {meeting.status === MeetingStatus.Scheduled && !isEditing && (
                    <>
                      <button onClick={() => setIsEditing(true)} className="btn-secondary">Editar</button>
                      <button onClick={handleStart} className="btn-success">Iniciar</button>
                    </>
                  )}
                  {isEditing && (
                    <>
                      <button onClick={handleSave} disabled={isSaving} className="btn-primary">{isSaving ? 'Salvando...' : 'Salvar'}</button>
                      <button onClick={() => setIsEditing(false)} className="btn-secondary">Cancelar</button>
                    </>
                  )}
                  {meeting.status === MeetingStatus.InProgress && <button onClick={handleEnd} className="btn-primary">Finalizar</button>}
                </div>
              </div>
            </div>

            {/* Tabs */}
            <div className="border-t border-gray-100 px-6">
              <div className="flex gap-6 overflow-x-auto scrollbar-hide">
                {[{ id: 'details', label: 'Detalhes' }, { id: 'agenda', label: `Pauta (${agendaItems.length})` }, { id: 'decisions', label: `Decisoes (${decisions.length})` }, { id: 'tasks', label: `Tarefas (${tasks.length})` }].map((tab) => (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id as typeof activeTab)}
                    className={`py-4 text-sm font-medium border-b-2 transition-colors whitespace-nowrap ${activeTab === tab.id ? 'border-indigo-600 text-indigo-600' : 'border-transparent text-gray-500 hover:text-gray-700'}`}
                  >
                    {tab.label}
                  </button>
                ))}
              </div>
            </div>
          </div>

          {/* Tab Content */}
          <div className="card p-6 animate-fadeIn">
            {/* Details */}
            {activeTab === 'details' && (
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                <div className="space-y-6">
                  <InfoBlock label="Descricao" isEditing={isEditing} value={isEditing ? editForm.description : meeting.description || 'Sem descricao'} onChange={(v) => setEditForm({ ...editForm, description: v })} multiline />
                  <InfoBlock label="Localizacao" isEditing={isEditing} value={isEditing ? editForm.location : meeting.location || 'Nao definida'} onChange={(v) => setEditForm({ ...editForm, location: v })} icon={<LocationIcon className="w-4 h-4 text-gray-400" />} />
                  <div>
                    <span className="text-xs font-medium text-gray-500 uppercase">Link da Reuniao</span>
                    {isEditing ? (
                      <input type="url" value={editForm.meetingUrl} onChange={(e) => setEditForm({ ...editForm, meetingUrl: e.target.value })} placeholder="https://..." className="mt-2" />
                    ) : meeting.meetingUrl ? (
                      <a href={meeting.meetingUrl} target="_blank" rel="noopener noreferrer" className="mt-2 inline-flex items-center gap-2 text-indigo-600 hover:text-indigo-700">
                        <LinkIcon className="w-4 h-4" />
                        Entrar na reuniao
                      </a>
                    ) : (
                      <p className="mt-2 text-gray-400">Nenhum link</p>
                    )}
                  </div>
                </div>

                <div>
                  <span className="text-xs font-medium text-gray-500 uppercase mb-3 block">Participantes ({meeting.participants.length})</span>
                  {meeting.participants.length === 0 ? (
                    <p className="text-gray-400">Nenhum participante</p>
                  ) : (
                    <div className="space-y-2">
                      {meeting.participants.map((p) => {
                        const pStatus = participantStatusConfig[p.status];
                        return (
                          <div key={p.id} className="flex items-center justify-between p-3 rounded-xl bg-gray-50">
                            <div className="flex items-center gap-3">
                              <div className="w-9 h-9 rounded-full bg-gradient-to-br from-indigo-500 to-violet-500 flex items-center justify-center text-white text-sm font-medium">
                                {p.userName?.charAt(0).toUpperCase() || 'U'}
                              </div>
                              <span className="text-sm font-medium text-gray-900">{p.userName}</span>
                            </div>
                            <span className={`badge ${pStatus.color}`}>{pStatus.label}</span>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Agenda */}
            {activeTab === 'agenda' && (
              <div>
                <div className="flex items-center justify-between mb-4">
                  <h3 className="section-title">Itens da Pauta</h3>
                  <button onClick={() => setShowAgendaForm(!showAgendaForm)} className="btn-primary text-sm py-2">
                    <PlusIcon className="w-4 h-4" /> Adicionar
                  </button>
                </div>

                {showAgendaForm && (
                  <FormCard onCancel={() => setShowAgendaForm(false)} onSubmit={handleAddAgendaItem}>
                    <input type="text" placeholder="Titulo do item" value={newAgendaItem.title} onChange={(e) => setNewAgendaItem({ ...newAgendaItem, title: e.target.value })} />
                    <textarea placeholder="Descricao (opcional)" value={newAgendaItem.description} onChange={(e) => setNewAgendaItem({ ...newAgendaItem, description: e.target.value })} rows={2} />
                    <div className="flex items-center gap-2">
                      <input type="number" value={newAgendaItem.estimatedMinutes} onChange={(e) => setNewAgendaItem({ ...newAgendaItem, estimatedMinutes: parseInt(e.target.value) || 15 })} className="w-20" />
                      <span className="text-sm text-gray-500">minutos</span>
                    </div>
                  </FormCard>
                )}

                {agendaItems.length === 0 ? (
                  <EmptyState message="Nenhum item na pauta" />
                ) : (
                  <div className="space-y-2">
                    {agendaItems.map((item, i) => (
                      <ChecklistItem
                        key={item.id}
                        checked={item.isCompleted}
                        onToggle={() => handleToggleAgendaItem(item)}
                        onDelete={() => handleDeleteAgendaItem(item)}
                        title={`${i + 1}. ${item.title}`}
                        subtitle={item.description}
                        badge={item.estimatedMinutes ? `${item.estimatedMinutes}min` : undefined}
                      />
                    ))}
                  </div>
                )}
              </div>
            )}

            {/* Decisions */}
            {activeTab === 'decisions' && (
              <div>
                <div className="flex items-center justify-between mb-4">
                  <h3 className="section-title">Decisoes</h3>
                  <button onClick={() => setShowDecisionForm(!showDecisionForm)} className="btn-primary text-sm py-2">
                    <PlusIcon className="w-4 h-4" /> Registrar
                  </button>
                </div>

                {showDecisionForm && (
                  <FormCard onCancel={() => setShowDecisionForm(false)} onSubmit={handleAddDecision}>
                    <input type="text" placeholder="Titulo da decisao" value={newDecision.title} onChange={(e) => setNewDecision({ ...newDecision, title: e.target.value })} />
                    <textarea placeholder="Descricao detalhada" value={newDecision.description} onChange={(e) => setNewDecision({ ...newDecision, description: e.target.value })} rows={2} />
                  </FormCard>
                )}

                {decisions.length === 0 ? (
                  <EmptyState message="Nenhuma decisao registrada" />
                ) : (
                  <div className="space-y-3">
                    {decisions.map((d) => (
                      <div key={d.id} className={`p-4 rounded-xl border ${d.isImplemented ? 'bg-emerald-50/50 border-emerald-100' : 'bg-white border-gray-100'}`}>
                        <div className="flex items-start justify-between gap-3">
                          <div className="flex-1">
                            <div className="flex items-center gap-2">
                              <h4 className={`font-medium ${d.isImplemented ? 'text-emerald-800' : 'text-gray-900'}`}>{d.title}</h4>
                              {d.isImplemented && <span className="badge bg-emerald-100 text-emerald-700">Implementada</span>}
                            </div>
                            <p className="text-sm text-gray-600 mt-1">{d.description}</p>
                            <p className="text-xs text-gray-400 mt-2">{formatDateTime(d.madeAt)}</p>
                          </div>
                          <div className="flex items-center gap-1">
                            <button onClick={() => handleToggleDecision(d)} className={`btn text-xs py-1.5 px-3 ${d.isImplemented ? 'btn-secondary' : 'btn-success'}`}>
                              {d.isImplemented ? 'Reverter' : 'Implementar'}
                            </button>
                            <button onClick={() => handleDeleteDecision(d)} className="btn-icon-danger"><TrashIcon className="w-4 h-4" /></button>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            )}

            {/* Tasks */}
            {activeTab === 'tasks' && (
              <div>
                <div className="flex items-center justify-between mb-4">
                  <h3 className="section-title">Tarefas</h3>
                  <button onClick={() => setShowTaskForm(!showTaskForm)} className="btn-primary text-sm py-2">
                    <PlusIcon className="w-4 h-4" /> Criar
                  </button>
                </div>

                {showTaskForm && (
                  <FormCard onCancel={() => setShowTaskForm(false)} onSubmit={handleAddTask}>
                    <input type="text" placeholder="Titulo da tarefa" value={newTask.title} onChange={(e) => setNewTask({ ...newTask, title: e.target.value })} />
                    <textarea placeholder="Descricao (opcional)" value={newTask.description} onChange={(e) => setNewTask({ ...newTask, description: e.target.value })} rows={2} />
                    <div className="grid grid-cols-3 gap-2">
                      <select value={selectedUserId} onChange={(e) => setSelectedUserId(e.target.value)}>
                        <option value="">Responsavel</option>
                        {users.map((u) => <option key={u.id} value={u.id}>{u.name}</option>)}
                      </select>
                      <select value={newTask.priority} onChange={(e) => setNewTask({ ...newTask, priority: e.target.value })}>
                        <option value="Low">Baixa</option>
                        <option value="Medium">Media</option>
                        <option value="High">Alta</option>
                        <option value="Critical">Critica</option>
                      </select>
                      <input type="date" value={newTask.dueDate} onChange={(e) => setNewTask({ ...newTask, dueDate: e.target.value })} />
                    </div>
                  </FormCard>
                )}

                {tasks.length === 0 ? (
                  <EmptyState message="Nenhuma tarefa criada" />
                ) : (
                  <div className="space-y-2">
                    {tasks.map((t) => (
                      <ChecklistItem
                        key={t.id}
                        checked={t.status === 'Completed'}
                        onToggle={() => t.status !== 'Completed' && handleCompleteTask(t)}
                        onDelete={() => handleDeleteTask(t)}
                        title={t.title}
                        subtitle={t.description}
                        badge={t.priority}
                        badgeColor={t.priority === 'Critical' ? 'bg-red-50 text-red-700' : t.priority === 'High' ? 'bg-amber-50 text-amber-700' : 'bg-gray-100 text-gray-600'}
                      />
                    ))}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Footer */}
          {meeting.status === MeetingStatus.Scheduled && !isEditing && (
            <div className="mt-6 flex items-center justify-between">
              <button onClick={handleDelete} className="btn-danger text-sm">Excluir Reuniao</button>
              <button onClick={handleCancel} className="btn-secondary text-sm">Cancelar Reuniao</button>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
}

// Components
function InfoBlock({ label, value, isEditing, onChange, multiline, icon }: { label: string; value: string; isEditing: boolean; onChange?: (v: string) => void; multiline?: boolean; icon?: React.ReactNode }) {
  return (
    <div>
      <span className="text-xs font-medium text-gray-500 uppercase">{label}</span>
      {isEditing ? (
        multiline ? <textarea value={value} onChange={(e) => onChange?.(e.target.value)} rows={3} className="mt-2" /> : <input type="text" value={value} onChange={(e) => onChange?.(e.target.value)} className="mt-2" />
      ) : (
        <p className="mt-2 text-gray-900 flex items-center gap-2">{icon}{value}</p>
      )}
    </div>
  );
}

function FormCard({ children, onCancel, onSubmit }: { children: React.ReactNode; onCancel: () => void; onSubmit: () => void }) {
  return (
    <div className="mb-4 p-4 rounded-xl bg-gray-50 space-y-3 animate-scaleIn">
      {children}
      <div className="flex justify-end gap-2 pt-2">
        <button onClick={onCancel} className="btn-ghost text-sm">Cancelar</button>
        <button onClick={onSubmit} className="btn-primary text-sm">Adicionar</button>
      </div>
    </div>
  );
}

function ChecklistItem({ checked, onToggle, onDelete, title, subtitle, badge, badgeColor }: { checked: boolean; onToggle: () => void; onDelete: () => void; title: string; subtitle?: string; badge?: string; badgeColor?: string }) {
  return (
    <div className={`flex items-start gap-3 p-3 rounded-xl transition-colors ${checked ? 'bg-emerald-50/50' : 'bg-gray-50 hover:bg-gray-100'}`}>
      <button onClick={onToggle} className={`mt-0.5 w-5 h-5 rounded border-2 flex items-center justify-center flex-shrink-0 transition-all ${checked ? 'bg-emerald-500 border-emerald-500' : 'border-gray-300 hover:border-emerald-500'}`}>
        {checked && <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={3} d="M5 13l4 4L19 7" /></svg>}
      </button>
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2">
          <span className={`font-medium ${checked ? 'text-gray-400 line-through' : 'text-gray-900'}`}>{title}</span>
          {badge && <span className={`badge ${badgeColor || 'bg-gray-100 text-gray-600'}`}>{badge}</span>}
        </div>
        {subtitle && <p className="text-sm text-gray-500 mt-0.5">{subtitle}</p>}
      </div>
      <button onClick={onDelete} className="btn-icon-danger"><TrashIcon className="w-4 h-4" /></button>
    </div>
  );
}

function EmptyState({ message }: { message: string }) {
  return <div className="text-center py-12 text-gray-400">{message}</div>;
}

// Icons
function ChevronLeftIcon({ className }: { className?: string }) { return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" /></svg>; }
function ClockIcon({ className }: { className?: string }) { return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>; }
function LocationIcon({ className }: { className?: string }) { return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17.657 16.657L13.414 20.9a1.998 1.998 0 01-2.827 0l-4.244-4.243a8 8 0 1111.314 0z" /></svg>; }
function LinkIcon({ className }: { className?: string }) { return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" /></svg>; }
function PlusIcon({ className }: { className?: string }) { return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>; }
function TrashIcon({ className }: { className?: string }) { return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" /></svg>; }
function AlertIcon({ className }: { className?: string }) { return <svg className={className} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" /></svg>; }
