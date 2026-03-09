import { AgentExecution, ExecutionStatus } from '../../../src/domain/agent/AgentExecution';

describe('AgentExecution', () => {
  const validProps = {
    agentId: 'agent-1',
    input: 'Hello, world!',
  };

  describe('constructor', () => {
    it('should create a pending execution by default', () => {
      const execution = new AgentExecution(validProps);

      expect(execution.id).toBeDefined();
      expect(execution.agentId).toBe('agent-1');
      expect(execution.input).toBe('Hello, world!');
      expect(execution.status).toBe(ExecutionStatus.PENDING);
      expect(execution.output).toBeUndefined();
      expect(execution.error).toBeUndefined();
    });
  });

  describe('lifecycle', () => {
    it('should transition from PENDING to RUNNING to COMPLETED', () => {
      const execution = new AgentExecution(validProps);

      execution.start();
      expect(execution.status).toBe(ExecutionStatus.RUNNING);

      execution.complete('The answer is 42', { model: 'gpt-4o' });
      expect(execution.status).toBe(ExecutionStatus.COMPLETED);
      expect(execution.output).toBe('The answer is 42');
      expect(execution.metadata).toMatchObject({ model: 'gpt-4o' });
    });

    it('should transition from RUNNING to FAILED', () => {
      const execution = new AgentExecution(validProps);

      execution.start();
      execution.fail('API error');
      expect(execution.status).toBe(ExecutionStatus.FAILED);
      expect(execution.error).toBe('API error');
    });
  });

  describe('toJSON', () => {
    it('should serialize to plain object', () => {
      const execution = new AgentExecution(validProps);
      const json = execution.toJSON();

      expect(json).toMatchObject({
        agentId: 'agent-1',
        input: 'Hello, world!',
        status: ExecutionStatus.PENDING,
      });
    });
  });
});
