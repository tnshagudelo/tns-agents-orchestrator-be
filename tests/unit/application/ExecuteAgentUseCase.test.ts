import { ExecuteAgentUseCase } from '../../../src/application/use-cases/ExecuteAgentUseCase';
import {
  GetExecutionUseCase,
  ListExecutionsByAgentUseCase,
} from '../../../src/application/use-cases/ExecutionQueryUseCases';
import { CreateAgentUseCase } from '../../../src/application/use-cases/CreateAgentUseCase';
import { InMemoryAgentRepository } from '../../../src/infrastructure/adapters/out/persistence/InMemoryAgentRepository';
import { InMemoryExecutionRepository } from '../../../src/infrastructure/adapters/out/persistence/InMemoryExecutionRepository';
import { InMemoryMessagePublisher } from '../../../src/infrastructure/adapters/out/messaging/InMemoryMessagePublisher';
import { ILLMProvider } from '../../../src/application/ports/out/ILLMProvider';
import { LLMRequest, LLMResponse } from '../../../src/domain/llm/LLMTypes';
import { ExecutionStatus } from '../../../src/domain/agent/AgentExecution';
import { AgentNotFoundError } from '../../../src/domain/shared/DomainError';

const mockLLMProvider: ILLMProvider = {
  providerName: 'mock',
  async complete(_req: LLMRequest): Promise<LLMResponse> {
    return {
      content: 'Mocked LLM response',
      model: 'mock-model',
      provider: 'mock',
      usage: { promptTokens: 10, completionTokens: 20, totalTokens: 30 },
    };
  },
  supportsModel: () => true,
};

const failingLLMProvider: ILLMProvider = {
  providerName: 'failing',
  async complete(_req: LLMRequest): Promise<LLMResponse> {
    throw new Error('LLM API error');
  },
  supportsModel: () => true,
};

describe('ExecuteAgentUseCase', () => {
  let agentRepository: InMemoryAgentRepository;
  let executionRepository: InMemoryExecutionRepository;
  let messagePublisher: InMemoryMessagePublisher;
  let createAgentUseCase: CreateAgentUseCase;
  let executeUseCase: ExecuteAgentUseCase;
  let getExecutionUseCase: GetExecutionUseCase;
  let listExecutionsByAgentUseCase: ListExecutionsByAgentUseCase;

  beforeEach(() => {
    agentRepository = new InMemoryAgentRepository();
    executionRepository = new InMemoryExecutionRepository();
    messagePublisher = new InMemoryMessagePublisher();
    createAgentUseCase = new CreateAgentUseCase(
      agentRepository,
      messagePublisher,
    );
    executeUseCase = new ExecuteAgentUseCase(
      agentRepository,
      executionRepository,
      mockLLMProvider,
      messagePublisher,
    );
    getExecutionUseCase = new GetExecutionUseCase(executionRepository);
    listExecutionsByAgentUseCase = new ListExecutionsByAgentUseCase(
      executionRepository,
    );
  });

  it('should execute agent and return completed execution', async () => {
    const agent = await createAgentUseCase.execute({
      name: 'Test Agent',
      description: 'Test',
      systemPrompt: 'You are helpful.',
    });

    const execution = await executeUseCase.execute({
      agentId: agent.id,
      input: 'Tell me a joke',
    });

    expect(execution.status).toBe(ExecutionStatus.COMPLETED);
    expect(execution.output).toBe('Mocked LLM response');
    expect(execution.agentId).toBe(agent.id);
  });

  it('should publish execution.started and execution.completed events', async () => {
    messagePublisher.clearEventLog();

    const agent = await createAgentUseCase.execute({
      name: 'Test Agent',
      description: 'Test',
      systemPrompt: 'You are helpful.',
    });

    messagePublisher.clearEventLog();

    await executeUseCase.execute({
      agentId: agent.id,
      input: 'Hello',
    });

    const events = messagePublisher.getEventLog();
    const eventTypes = events.map((e) => e.type);
    expect(eventTypes).toContain('execution.started');
    expect(eventTypes).toContain('execution.completed');
  });

  it('should fail execution when LLM provider throws', async () => {
    const failingExecuteUseCase = new ExecuteAgentUseCase(
      agentRepository,
      executionRepository,
      failingLLMProvider,
      messagePublisher,
    );

    const agent = await createAgentUseCase.execute({
      name: 'Test Agent',
      description: 'Test',
      systemPrompt: 'You are helpful.',
    });

    const execution = await failingExecuteUseCase.execute({
      agentId: agent.id,
      input: 'Hello',
    });

    expect(execution.status).toBe(ExecutionStatus.FAILED);
    expect(execution.error).toBe('LLM API error');
  });

  it('should throw AgentNotFoundError for non-existent agent', async () => {
    await expect(
      executeUseCase.execute({ agentId: 'non-existent', input: 'Hi' }),
    ).rejects.toThrow(AgentNotFoundError);
  });

  describe('GetExecutionUseCase', () => {
    it('should return execution by id', async () => {
      const agent = await createAgentUseCase.execute({
        name: 'Agent',
        description: 'Desc',
        systemPrompt: 'Prompt',
      });
      const execution = await executeUseCase.execute({
        agentId: agent.id,
        input: 'Hi',
      });

      const found = await getExecutionUseCase.execute(execution.id);
      expect(found.id).toBe(execution.id);
    });
  });

  describe('ListExecutionsByAgentUseCase', () => {
    it('should list executions for a specific agent', async () => {
      const agent = await createAgentUseCase.execute({
        name: 'Agent',
        description: 'Desc',
        systemPrompt: 'Prompt',
      });

      await executeUseCase.execute({ agentId: agent.id, input: 'q1' });
      await executeUseCase.execute({ agentId: agent.id, input: 'q2' });

      const executions = await listExecutionsByAgentUseCase.execute(agent.id);
      expect(executions).toHaveLength(2);
    });
  });
});
