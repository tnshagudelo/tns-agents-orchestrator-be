import { AgentExecution } from '../../../domain/agent/AgentExecution';

export interface IExecutionRepository {
  save(execution: AgentExecution): Promise<void>;
  findById(id: string): Promise<AgentExecution | null>;
  findByAgentId(agentId: string): Promise<AgentExecution[]>;
  findPending(): Promise<AgentExecution[]>;
}
