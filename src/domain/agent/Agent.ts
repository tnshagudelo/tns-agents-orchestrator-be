import { UniqueId } from '../shared/UniqueId';
import { InvalidAgentError } from '../shared/DomainError';

export enum AgentStatus {
  ACTIVE = 'active',
  INACTIVE = 'inactive',
}

export interface AgentProps {
  id?: string;
  name: string;
  description: string;
  systemPrompt: string;
  model?: string;
  provider?: string;
  capabilities?: string[];
  createdAt?: Date;
  updatedAt?: Date;
}

export class Agent {
  private readonly _id: UniqueId;
  private _name: string;
  private _description: string;
  private _systemPrompt: string;
  private _model: string;
  private _provider: string;
  private _capabilities: string[];
  private _status: AgentStatus;
  private readonly _createdAt: Date;
  private _updatedAt: Date;

  constructor(props: AgentProps) {
    if (!props.name || props.name.trim().length === 0) {
      throw new InvalidAgentError('name cannot be empty');
    }
    if (!props.description || props.description.trim().length === 0) {
      throw new InvalidAgentError('description cannot be empty');
    }
    if (!props.systemPrompt || props.systemPrompt.trim().length === 0) {
      throw new InvalidAgentError('systemPrompt cannot be empty');
    }

    this._id = new UniqueId(props.id);
    this._name = props.name.trim();
    this._description = props.description.trim();
    this._systemPrompt = props.systemPrompt.trim();
    this._model = props.model ?? 'gpt-4o';
    this._provider = props.provider ?? 'openai';
    this._capabilities = props.capabilities ?? [];
    this._status = AgentStatus.ACTIVE;
    this._createdAt = props.createdAt ?? new Date();
    this._updatedAt = props.updatedAt ?? new Date();
  }

  get id(): string {
    return this._id.value;
  }

  get name(): string {
    return this._name;
  }

  get description(): string {
    return this._description;
  }

  get systemPrompt(): string {
    return this._systemPrompt;
  }

  get model(): string {
    return this._model;
  }

  get provider(): string {
    return this._provider;
  }

  get capabilities(): string[] {
    return [...this._capabilities];
  }

  get status(): AgentStatus {
    return this._status;
  }

  get createdAt(): Date {
    return this._createdAt;
  }

  get updatedAt(): Date {
    return this._updatedAt;
  }

  activate(): void {
    this._status = AgentStatus.ACTIVE;
    this._updatedAt = new Date();
  }

  deactivate(): void {
    this._status = AgentStatus.INACTIVE;
    this._updatedAt = new Date();
  }

  update(props: Partial<Pick<AgentProps, 'name' | 'description' | 'systemPrompt' | 'model' | 'provider' | 'capabilities'>>): void {
    if (props.name !== undefined) {
      if (props.name.trim().length === 0) {
        throw new InvalidAgentError('name cannot be empty');
      }
      this._name = props.name.trim();
    }
    if (props.description !== undefined) {
      if (props.description.trim().length === 0) {
        throw new InvalidAgentError('description cannot be empty');
      }
      this._description = props.description.trim();
    }
    if (props.systemPrompt !== undefined) {
      if (props.systemPrompt.trim().length === 0) {
        throw new InvalidAgentError('systemPrompt cannot be empty');
      }
      this._systemPrompt = props.systemPrompt.trim();
    }
    if (props.model !== undefined) {
      this._model = props.model;
    }
    if (props.provider !== undefined) {
      this._provider = props.provider;
    }
    if (props.capabilities !== undefined) {
      this._capabilities = [...props.capabilities];
    }
    this._updatedAt = new Date();
  }

  toJSON() {
    return {
      id: this._id.value,
      name: this._name,
      description: this._description,
      systemPrompt: this._systemPrompt,
      model: this._model,
      provider: this._provider,
      capabilities: this._capabilities,
      status: this._status,
      createdAt: this._createdAt,
      updatedAt: this._updatedAt,
    };
  }
}
