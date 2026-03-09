import { Agent } from '../../../domain/agent/Agent';

export interface IGetAgentUseCase {
  execute(agentId: string): Promise<Agent>;
}
