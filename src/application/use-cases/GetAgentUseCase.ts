import { Agent } from '../../domain/agent/Agent';
import { IGetAgentUseCase } from '../ports/in/IGetAgentUseCase';
import { IAgentRepository } from '../ports/out/IAgentRepository';
import { AgentNotFoundError } from '../../domain/shared/DomainError';

export class GetAgentUseCase implements IGetAgentUseCase {
  constructor(private readonly agentRepository: IAgentRepository) {}

  async execute(agentId: string): Promise<Agent> {
    const agent = await this.agentRepository.findById(agentId);
    if (!agent) {
      throw new AgentNotFoundError(agentId);
    }
    return agent;
  }
}
