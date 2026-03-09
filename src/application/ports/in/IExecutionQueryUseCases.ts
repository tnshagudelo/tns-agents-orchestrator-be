import { AgentExecution } from '../../../domain/agent/AgentExecution';

export interface IGetExecutionUseCase {
  execute(executionId: string): Promise<AgentExecution>;
}

export interface IListExecutionsByAgentUseCase {
  execute(agentId: string): Promise<AgentExecution[]>;
}
