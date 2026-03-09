import { UniqueId } from '../shared/UniqueId';

export enum ExecutionStatus {
  PENDING = 'pending',
  RUNNING = 'running',
  COMPLETED = 'completed',
  FAILED = 'failed',
}

export interface AgentExecutionProps {
  id?: string;
  agentId: string;
  input: string;
  output?: string;
  status?: ExecutionStatus;
  error?: string;
  metadata?: Record<string, unknown>;
  createdAt?: Date;
  updatedAt?: Date;
}

export class AgentExecution {
  private readonly _id: UniqueId;
  private readonly _agentId: string;
  private _input: string;
  private _output: string | undefined;
  private _status: ExecutionStatus;
  private _error: string | undefined;
  private _metadata: Record<string, unknown>;
  private readonly _createdAt: Date;
  private _updatedAt: Date;

  constructor(props: AgentExecutionProps) {
    this._id = new UniqueId(props.id);
    this._agentId = props.agentId;
    this._input = props.input;
    this._output = props.output;
    this._status = props.status ?? ExecutionStatus.PENDING;
    this._error = props.error;
    this._metadata = props.metadata ?? {};
    this._createdAt = props.createdAt ?? new Date();
    this._updatedAt = props.updatedAt ?? new Date();
  }

  get id(): string {
    return this._id.value;
  }

  get agentId(): string {
    return this._agentId;
  }

  get input(): string {
    return this._input;
  }

  get output(): string | undefined {
    return this._output;
  }

  get status(): ExecutionStatus {
    return this._status;
  }

  get error(): string | undefined {
    return this._error;
  }

  get metadata(): Record<string, unknown> {
    return { ...this._metadata };
  }

  get createdAt(): Date {
    return this._createdAt;
  }

  get updatedAt(): Date {
    return this._updatedAt;
  }

  start(): void {
    this._status = ExecutionStatus.RUNNING;
    this._updatedAt = new Date();
  }

  complete(output: string, metadata?: Record<string, unknown>): void {
    this._status = ExecutionStatus.COMPLETED;
    this._output = output;
    if (metadata) {
      this._metadata = { ...this._metadata, ...metadata };
    }
    this._updatedAt = new Date();
  }

  fail(error: string): void {
    this._status = ExecutionStatus.FAILED;
    this._error = error;
    this._updatedAt = new Date();
  }

  toJSON() {
    return {
      id: this._id.value,
      agentId: this._agentId,
      input: this._input,
      output: this._output,
      status: this._status,
      error: this._error,
      metadata: this._metadata,
      createdAt: this._createdAt,
      updatedAt: this._updatedAt,
    };
  }
}
