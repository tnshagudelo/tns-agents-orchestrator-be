import { CreateAgentUseCase } from '../../../src/application/use-cases/CreateAgentUseCase';
import { GetAgentUseCase } from '../../../src/application/use-cases/GetAgentUseCase';
import { ListAgentsUseCase } from '../../../src/application/use-cases/ListAgentsUseCase';
import { InMemoryAgentRepository } from '../../../src/infrastructure/adapters/out/persistence/InMemoryAgentRepository';
import { InMemoryMessagePublisher } from '../../../src/infrastructure/adapters/out/messaging/InMemoryMessagePublisher';
import { AgentNotFoundError } from '../../../src/domain/shared/DomainError';

describe('Agent Use Cases', () => {
  let agentRepository: InMemoryAgentRepository;
  let messagePublisher: InMemoryMessagePublisher;
  let createUseCase: CreateAgentUseCase;
  let getUseCase: GetAgentUseCase;
  let listUseCase: ListAgentsUseCase;

  beforeEach(() => {
    agentRepository = new InMemoryAgentRepository();
    messagePublisher = new InMemoryMessagePublisher();
    createUseCase = new CreateAgentUseCase(agentRepository, messagePublisher);
    getUseCase = new GetAgentUseCase(agentRepository);
    listUseCase = new ListAgentsUseCase(agentRepository);
  });

  describe('CreateAgentUseCase', () => {
    it('should create an agent and save it', async () => {
      const agent = await createUseCase.execute({
        name: 'My Agent',
        description: 'Does things',
        systemPrompt: 'You are helpful.',
      });

      expect(agent.id).toBeDefined();
      expect(agent.name).toBe('My Agent');

      const saved = await agentRepository.findById(agent.id);
      expect(saved).not.toBeNull();
    });

    it('should publish agent.created event after creation', async () => {
      await createUseCase.execute({
        name: 'My Agent',
        description: 'Does things',
        systemPrompt: 'You are helpful.',
      });

      const events = messagePublisher.getEventLog();
      expect(events).toHaveLength(1);
      expect(events[0].type).toBe('agent.created');
    });

    it('should create agent with custom provider and model', async () => {
      const agent = await createUseCase.execute({
        name: 'Claude Agent',
        description: 'Uses Anthropic',
        systemPrompt: 'You are Claude.',
        provider: 'anthropic',
        model: 'claude-3-5-sonnet-20241022',
      });

      expect(agent.provider).toBe('anthropic');
      expect(agent.model).toBe('claude-3-5-sonnet-20241022');
    });
  });

  describe('GetAgentUseCase', () => {
    it('should return an agent by id', async () => {
      const created = await createUseCase.execute({
        name: 'My Agent',
        description: 'Does things',
        systemPrompt: 'You are helpful.',
      });

      const found = await getUseCase.execute(created.id);
      expect(found.id).toBe(created.id);
    });

    it('should throw AgentNotFoundError for non-existent id', async () => {
      await expect(getUseCase.execute('non-existent-id')).rejects.toThrow(
        AgentNotFoundError,
      );
    });
  });

  describe('ListAgentsUseCase', () => {
    it('should return all agents', async () => {
      await createUseCase.execute({
        name: 'Agent 1',
        description: 'First',
        systemPrompt: 'System 1',
      });
      await createUseCase.execute({
        name: 'Agent 2',
        description: 'Second',
        systemPrompt: 'System 2',
      });

      const agents = await listUseCase.execute();
      expect(agents).toHaveLength(2);
    });

    it('should return empty array when no agents exist', async () => {
      const agents = await listUseCase.execute();
      expect(agents).toEqual([]);
    });
  });
});
