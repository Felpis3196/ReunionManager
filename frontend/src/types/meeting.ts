export enum MeetingType {
  Planning = 'Planning',
  Review = 'Review',
  Standup = 'Standup',
  Retrospective = 'Retrospective',
  OneOnOne = 'OneOnOne',
  Other = 'Other',
}

export enum MeetingStatus {
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
}

export enum ParticipantStatus {
  Invited = 'Invited',
  Accepted = 'Accepted',
  Declined = 'Declined',
  Attended = 'Attended',
  Absent = 'Absent',
}

export interface Participant {
  id: string;
  userId: string;
  userName: string;
  userEmail: string;
  status: ParticipantStatus;
  invitedAt?: string;
  attendedAt?: string;
}

export interface AgendaItem {
  id: string;
  order: number;
  title: string;
  description?: string;
  estimatedDuration?: string;
  isCompleted: boolean;
}

export interface Decision {
  id: string;
  title: string;
  description: string;
  madeAt: string;
  madeById?: string;
  madeByName?: string;
  isImplemented: boolean;
}

export interface Task {
  id: string;
  meetingId: string;
  projectId?: string;
  assignedToId: string;
  assignedToName: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string;
  completedAt?: string;
}

export enum TaskStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
}

export enum TaskPriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Critical = 'Critical',
}

export interface Meeting {
  id: string;
  organizationId: string;
  projectId?: string;
  organizerId: string;
  title: string;
  description?: string;
  type: MeetingType;
  status: MeetingStatus;
  scheduledAt: string;
  duration: string;
  location?: string;
  meetingUrl?: string;
  suggestedAgenda?: string;
  finalAgenda?: string;
  summary?: string;
  createdAt: string;
  startedAt?: string;
  endedAt?: string;
  projectName?: string;
  organizerName: string;
  participants: Participant[];
  agendaItems: AgendaItem[];
  decisions: Decision[];
  tasks: Task[];
}

export interface CreateMeetingDto {
  organizationId: string;
  projectId?: string;
  title: string;
  description?: string;
  type: MeetingType;
  scheduledAt: string;
  duration: string;
  location?: string;
  meetingUrl?: string;
  participantIds: string[];
}

export interface UpdateMeetingDto {
  title?: string;
  description?: string;
  type?: MeetingType;
  scheduledAt?: string;
  duration?: string;
  location?: string;
  meetingUrl?: string;
  finalAgenda?: string;
}
