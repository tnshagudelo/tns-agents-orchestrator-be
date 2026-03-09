import { AgentExecution } from '../../domain/agent/AgentExecution';
import { IExecuteAgentUseCase, ExecuteAgentCommand } from '../ports/in/IExecuteAgentUseCase';
import { IAgentRepository } from '../ports/out/IAgentRepository';
import { IExecutionRepository } from '../ports/out/IExecutionRepository';
import { ILLMProvider } from '../ports/out/ILLMProvider';
import { IMessagePublisher } from '../ports/out/IMessagePublisher';
import { AgentNotFoundError } from '../../domain/shared/DomainError';
import { MessageRole } from '../../domain/llm/LLMTypes';

export class ExecuteAgentUseCase implements IExecuteAgentUseCase {
  constructor(
    private readonly agentRepository: IAgentRepository,
    private readonly executionRepository: IExecutionRepository,
    private readonly llmProvider: ILLMProvider,
    private readonly messagePublisher: IMessagePublisher,
  ) {}

  async execute(command: ExecuteAgentCommand): Promise<AgentExecution> {
    const agent = await this.agentRepository.findById(command.agentId);
    if (!agent) {
      throw new AgentNotFoundError(command.agentId);
    }

    const execution = new AgentExecution({
      agentId: agent.id,
      input: command.input,
      metadata: command.metadata,
    });

    await this.executionRepository.save(execution);

    await this.messagePublisher.publish({
      type: 'execution.started',
      payload: { executionId: execution.id, agentId: agent.id },
      timestamp: new Date(),
    });

    try {
      execution.start();
      await this.executionRepository.save(execution);

      const llmResponse = await this.llmProvider.complete({
        messages: [
          { role: MessageRole.SYSTEM, content: agent.systemPrompt },
          { role: MessageRole.USER, content: command.input },
        ],
        model: agent.model,
        provider: agent.provider,
      });

      execution.complete(llmResponse.content, {
        model: llmResponse.model,
        provider: llmResponse.provider,
        usage: llmResponse.usage,
      });

      await this.executionRepository.save(execution);

      await this.messagePublisher.publish({
        type: 'execution.completed',
        payload: {
          executionId: execution.id,
          agentId: agent.id,
          output: execution.output,
        },
        timestamp: new Date(),
      });
    } catch (error) {
      const errorMessage =
        error instanceof Error ? error.message : 'Unknown error';
      execution.fail(errorMessage);
      await this.executionRepository.save(execution);

      await this.messagePublisher.publish({
        type: 'execution.failed',
        payload: {
          executionId: execution.id,
          agentId: agent.id,
          error: errorMessage,
        },
        timestamp: new Date(),
      });
    }

    return execution;
  }
}
