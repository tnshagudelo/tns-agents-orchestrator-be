import { AgentExecution } from '../../../domain/agent/AgentExecution';

export interface ExecuteAgentCommand {
  agentId: string;
  input: string;
  metadata?: Record<string, unknown>;
}

export interface IExecuteAgentUseCase {
  execute(command: ExecuteAgentCommand): Promise<AgentExecution>;
}
