import { Agent } from '../../domain/agent/Agent';
import { ICreateAgentUseCase, CreateAgentCommand } from '../ports/in/ICreateAgentUseCase';
import { IAgentRepository } from '../ports/out/IAgentRepository';
import { IMessagePublisher } from '../ports/out/IMessagePublisher';

export class CreateAgentUseCase implements ICreateAgentUseCase {
  constructor(
    private readonly agentRepository: IAgentRepository,
    private readonly messagePublisher: IMessagePublisher,
  ) {}

  async execute(command: CreateAgentCommand): Promise<Agent> {
    const agent = new Agent({
      name: command.name,
      description: command.description,
      systemPrompt: command.systemPrompt,
      model: command.model,
      provider: command.provider,
      capabilities: command.capabilities,
    });

    await this.agentRepository.save(agent);

    await this.messagePublisher.publish({
      type: 'agent.created',
      payload: { agentId: agent.id, name: agent.name },
      timestamp: new Date(),
    });

    return agent;
  }
}
