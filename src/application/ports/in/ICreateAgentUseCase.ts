import { Agent } from '../../../domain/agent/Agent';

export interface CreateAgentCommand {
  name: string;
  description: string;
  systemPrompt: string;
  model?: string;
  provider?: string;
  capabilities?: string[];
}

export interface ICreateAgentUseCase {
  execute(command: CreateAgentCommand): Promise<Agent>;
}
