import { Agent } from '../../domain/agent/Agent';
import { IListAgentsUseCase } from '../ports/in/IListAgentsUseCase';
import { IAgentRepository } from '../ports/out/IAgentRepository';

export class ListAgentsUseCase implements IListAgentsUseCase {
  constructor(private readonly agentRepository: IAgentRepository) {}

  async execute(): Promise<Agent[]> {
    return this.agentRepository.findAll();
  }
}
