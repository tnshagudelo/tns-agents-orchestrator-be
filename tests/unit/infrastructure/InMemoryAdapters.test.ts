import { InMemoryAgentRepository } from '../../../src/infrastructure/adapters/out/persistence/InMemoryAgentRepository';
import { InMemoryExecutionRepository } from '../../../src/infrastructure/adapters/out/persistence/InMemoryExecutionRepository';
import { InMemoryMessagePublisher } from '../../../src/infrastructure/adapters/out/messaging/InMemoryMessagePublisher';
import { Agent } from '../../../src/domain/agent/Agent';
import { AgentExecution } from '../../../src/domain/agent/AgentExecution';

describe('InMemoryAgentRepository', () => {
  let repo: InMemoryAgentRepository;

  const makeAgent = (name = 'Agent') =>
    new Agent({ name, description: 'Desc', systemPrompt: 'Prompt' });

  beforeEach(() => {
    repo = new InMemoryAgentRepository();
  });

  it('should save and find an agent by id', async () => {
    const agent = makeAgent();
    await repo.save(agent);
    const found = await repo.findById(agent.id);
    expect(found).not.toBeNull();
    expect(found?.id).toBe(agent.id);
  });

  it('should return null for non-existent id', async () => {
    const found = await repo.findById('missing');
    expect(found).toBeNull();
  });

  it('should return all agents', async () => {
    await repo.save(makeAgent('A'));
    await repo.save(makeAgent('B'));
    const all = await repo.findAll();
    expect(all).toHaveLength(2);
  });

  it('should delete an agent', async () => {
    const agent = makeAgent();
    await repo.save(agent);
    await repo.delete(agent.id);
    const found = await repo.findById(agent.id);
    expect(found).toBeNull();
  });
});

describe('InMemoryExecutionRepository', () => {
  let repo: InMemoryExecutionRepository;

  const makeExecution = (agentId = 'agent-1') =>
    new AgentExecution({ agentId, input: 'test input' });

  beforeEach(() => {
    repo = new InMemoryExecutionRepository();
  });

  it('should save and find an execution', async () => {
    const exec = makeExecution();
    await repo.save(exec);
    const found = await repo.findById(exec.id);
    expect(found?.id).toBe(exec.id);
  });

  it('should find executions by agentId', async () => {
    await repo.save(makeExecution('agent-1'));
    await repo.save(makeExecution('agent-1'));
    await repo.save(makeExecution('agent-2'));

    const results = await repo.findByAgentId('agent-1');
    expect(results).toHaveLength(2);
  });

  it('should find pending executions', async () => {
    const pending = makeExecution();
    const running = makeExecution();
    running.start();

    await repo.save(pending);
    await repo.save(running);

    const pendingList = await repo.findPending();
    expect(pendingList).toHaveLength(1);
    expect(pendingList[0].id).toBe(pending.id);
  });
});

describe('InMemoryMessagePublisher', () => {
  let publisher: InMemoryMessagePublisher;

  beforeEach(() => {
    publisher = new InMemoryMessagePublisher();
  });

  it('should publish events to log', async () => {
    await publisher.publish({
      type: 'test.event',
      payload: { key: 'value' },
      timestamp: new Date(),
    });

    const log = publisher.getEventLog();
    expect(log).toHaveLength(1);
    expect(log[0].type).toBe('test.event');
  });

  it('should call subscribed handlers on publish', async () => {
    const handler = jest.fn().mockResolvedValue(undefined);
    publisher.subscribe('test.event', handler);

    await publisher.publish({
      type: 'test.event',
      payload: {},
      timestamp: new Date(),
    });

    expect(handler).toHaveBeenCalledTimes(1);
  });

  it('should not call handlers for different event types', async () => {
    const handler = jest.fn().mockResolvedValue(undefined);
    publisher.subscribe('other.event', handler);

    await publisher.publish({
      type: 'test.event',
      payload: {},
      timestamp: new Date(),
    });

    expect(handler).not.toHaveBeenCalled();
  });

  it('should clear event log', async () => {
    await publisher.publish({
      type: 'test.event',
      payload: {},
      timestamp: new Date(),
    });
    publisher.clearEventLog();
    expect(publisher.getEventLog()).toHaveLength(0);
  });
});
