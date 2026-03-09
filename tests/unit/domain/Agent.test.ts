import { Agent, AgentStatus } from '../../../src/domain/agent/Agent';
import {
  InvalidAgentError,
  AgentNotFoundError,
} from '../../../src/domain/shared/DomainError';

describe('Agent', () => {
  const validProps = {
    name: 'Test Agent',
    description: 'A test agent',
    systemPrompt: 'You are a helpful assistant.',
  };

  describe('constructor', () => {
    it('should create a valid agent with required props', () => {
      const agent = new Agent(validProps);

      expect(agent.id).toBeDefined();
      expect(agent.name).toBe('Test Agent');
      expect(agent.description).toBe('A test agent');
      expect(agent.systemPrompt).toBe('You are a helpful assistant.');
      expect(agent.model).toBe('gpt-4o');
      expect(agent.provider).toBe('openai');
      expect(agent.capabilities).toEqual([]);
      expect(agent.status).toBe(AgentStatus.ACTIVE);
      expect(agent.createdAt).toBeInstanceOf(Date);
      expect(agent.updatedAt).toBeInstanceOf(Date);
    });

    it('should create agent with custom props', () => {
      const agent = new Agent({
        ...validProps,
        model: 'claude-3-5-sonnet-20241022',
        provider: 'anthropic',
        capabilities: ['summarize', 'translate'],
      });

      expect(agent.model).toBe('claude-3-5-sonnet-20241022');
      expect(agent.provider).toBe('anthropic');
      expect(agent.capabilities).toEqual(['summarize', 'translate']);
    });

    it('should throw InvalidAgentError when name is empty', () => {
      expect(() => new Agent({ ...validProps, name: '' })).toThrow(
        InvalidAgentError,
      );
    });

    it('should throw InvalidAgentError when description is empty', () => {
      expect(
        () => new Agent({ ...validProps, description: '' }),
      ).toThrow(InvalidAgentError);
    });

    it('should throw InvalidAgentError when systemPrompt is empty', () => {
      expect(
        () => new Agent({ ...validProps, systemPrompt: '' }),
      ).toThrow(InvalidAgentError);
    });

    it('should trim whitespace from string props', () => {
      const agent = new Agent({
        name: '  Trimmed Name  ',
        description: '  Trimmed Desc  ',
        systemPrompt: '  Trimmed Prompt  ',
      });

      expect(agent.name).toBe('Trimmed Name');
      expect(agent.description).toBe('Trimmed Desc');
      expect(agent.systemPrompt).toBe('Trimmed Prompt');
    });

    it('should accept a specific id', () => {
      const id = '550e8400-e29b-41d4-a716-446655440000';
      const agent = new Agent({ ...validProps, id });
      expect(agent.id).toBe(id);
    });
  });

  describe('activate/deactivate', () => {
    it('should deactivate an active agent', () => {
      const agent = new Agent(validProps);
      expect(agent.status).toBe(AgentStatus.ACTIVE);

      agent.deactivate();
      expect(agent.status).toBe(AgentStatus.INACTIVE);
    });

    it('should activate an inactive agent', () => {
      const agent = new Agent(validProps);
      agent.deactivate();
      agent.activate();
      expect(agent.status).toBe(AgentStatus.ACTIVE);
    });
  });

  describe('update', () => {
    it('should update name', () => {
      const agent = new Agent(validProps);
      agent.update({ name: 'Updated Name' });
      expect(agent.name).toBe('Updated Name');
    });

    it('should throw InvalidAgentError when updating with empty name', () => {
      const agent = new Agent(validProps);
      expect(() => agent.update({ name: '' })).toThrow(InvalidAgentError);
    });

    it('should update multiple fields at once', () => {
      const agent = new Agent(validProps);
      agent.update({
        name: 'New Name',
        provider: 'anthropic',
        model: 'claude-3-opus-20240229',
      });
      expect(agent.name).toBe('New Name');
      expect(agent.provider).toBe('anthropic');
      expect(agent.model).toBe('claude-3-opus-20240229');
    });
  });

  describe('toJSON', () => {
    it('should return a plain object representation', () => {
      const agent = new Agent(validProps);
      const json = agent.toJSON();

      expect(json).toMatchObject({
        id: agent.id,
        name: 'Test Agent',
        description: 'A test agent',
        systemPrompt: 'You are a helpful assistant.',
        model: 'gpt-4o',
        provider: 'openai',
        capabilities: [],
        status: AgentStatus.ACTIVE,
      });
      expect(json.createdAt).toBeInstanceOf(Date);
      expect(json.updatedAt).toBeInstanceOf(Date);
    });
  });

  describe('DomainErrors', () => {
    it('AgentNotFoundError should have correct code', () => {
      const err = new AgentNotFoundError('test-id');
      expect(err.code).toBe('AGENT_NOT_FOUND');
      expect(err.message).toContain('test-id');
    });
  });
});
