import request from 'supertest';
import { createApp } from '../../../src/app';
import { AgentController } from '../../../src/infrastructure/adapters/in/http/AgentController';
import { ExecutionController } from '../../../src/infrastructure/adapters/in/http/ExecutionController';
import { HealthController } from '../../../src/infrastructure/adapters/in/http/HealthController';
import { CreateAgentUseCase } from '../../../src/application/use-cases/CreateAgentUseCase';
import { GetAgentUseCase } from '../../../src/application/use-cases/GetAgentUseCase';
import { ListAgentsUseCase } from '../../../src/application/use-cases/ListAgentsUseCase';
import { ExecuteAgentUseCase } from '../../../src/application/use-cases/ExecuteAgentUseCase';
import {
  GetExecutionUseCase,
  ListExecutionsByAgentUseCase,
} from '../../../src/application/use-cases/ExecutionQueryUseCases';
import { InMemoryAgentRepository } from '../../../src/infrastructure/adapters/out/persistence/InMemoryAgentRepository';
import { InMemoryExecutionRepository } from '../../../src/infrastructure/adapters/out/persistence/InMemoryExecutionRepository';
import { InMemoryMessagePublisher } from '../../../src/infrastructure/adapters/out/messaging/InMemoryMessagePublisher';
import { ILLMProvider } from '../../../src/application/ports/out/ILLMProvider';
import { LLMRequest, LLMResponse } from '../../../src/domain/llm/LLMTypes';

const mockLLMProvider: ILLMProvider = {
  providerName: 'mock',
  async complete(_req: LLMRequest): Promise<LLMResponse> {
    return {
      content: 'Mock response',
      model: 'mock-model',
      provider: 'mock',
      usage: { promptTokens: 5, completionTokens: 10, totalTokens: 15 },
    };
  },
  supportsModel: () => true,
};

function buildTestApp() {
  const agentRepo = new InMemoryAgentRepository();
  const executionRepo = new InMemoryExecutionRepository();
  const publisher = new InMemoryMessagePublisher();

  const createAgent = new CreateAgentUseCase(agentRepo, publisher);
  const getAgent = new GetAgentUseCase(agentRepo);
  const listAgents = new ListAgentsUseCase(agentRepo);
  const executeAgent = new ExecuteAgentUseCase(
    agentRepo,
    executionRepo,
    mockLLMProvider,
    publisher,
  );
  const getExecution = new GetExecutionUseCase(executionRepo);
  const listExecutions = new ListExecutionsByAgentUseCase(executionRepo);

  const agentController = new AgentController(createAgent, getAgent, listAgents);
  const executionController = new ExecutionController(
    executeAgent,
    getExecution,
    listExecutions,
  );
  const healthController = new HealthController();

  return createApp(agentController, executionController, healthController);
}

describe('API Integration Tests', () => {
  const app = buildTestApp();

  describe('GET /health', () => {
    it('should return 200 with status ok', async () => {
      const res = await request(app).get('/health');
      expect(res.status).toBe(200);
      expect(res.body.status).toBe('ok');
      expect(res.body.service).toBe('tns-agents-orchestrator-be');
    });
  });

  describe('Agents API', () => {
    it('GET /api/v1/agents should return empty list initially', async () => {
      const res = await request(app).get('/api/v1/agents');
      expect(res.status).toBe(200);
      expect(res.body.data).toEqual([]);
    });

    it('POST /api/v1/agents should create an agent', async () => {
      const res = await request(app)
        .post('/api/v1/agents')
        .send({
          name: 'Integration Agent',
          description: 'Test agent',
          systemPrompt: 'You are helpful.',
          provider: 'openai',
          model: 'gpt-4o',
        });

      expect(res.status).toBe(201);
      expect(res.body.data.name).toBe('Integration Agent');
      expect(res.body.data.id).toBeDefined();
    });

    it('POST /api/v1/agents should return 400 for invalid body', async () => {
      const res = await request(app)
        .post('/api/v1/agents')
        .send({ name: '' });

      expect(res.status).toBe(400);
      expect(res.body.error).toBe('Validation failed');
    });

    it('GET /api/v1/agents/:id should return the created agent', async () => {
      const createRes = await request(app)
        .post('/api/v1/agents')
        .send({
          name: 'Agent to Get',
          description: 'For retrieval',
          systemPrompt: 'Be precise.',
        });

      const agentId = createRes.body.data.id;
      const getRes = await request(app).get(`/api/v1/agents/${agentId}`);

      expect(getRes.status).toBe(200);
      expect(getRes.body.data.id).toBe(agentId);
    });

    it('GET /api/v1/agents/:id should return 404 for non-existent agent', async () => {
      const res = await request(app).get('/api/v1/agents/non-existent-id');
      expect(res.status).toBe(404);
    });
  });

  describe('Executions API', () => {
    let agentId: string;

    beforeEach(async () => {
      const res = await request(app)
        .post('/api/v1/agents')
        .send({
          name: 'Execution Test Agent',
          description: 'For execution tests',
          systemPrompt: 'Answer questions.',
        });
      agentId = res.body.data.id;
    });

    it('POST /api/v1/agents/:agentId/executions should execute agent', async () => {
      const res = await request(app)
        .post(`/api/v1/agents/${agentId}/executions`)
        .send({ input: 'What is 2+2?' });

      expect(res.status).toBe(201);
      expect(res.body.data.output).toBe('Mock response');
      expect(res.body.data.agentId).toBe(agentId);
    });

    it('POST /api/v1/agents/:agentId/executions should return 400 for empty input', async () => {
      const res = await request(app)
        .post(`/api/v1/agents/${agentId}/executions`)
        .send({ input: '' });

      expect(res.status).toBe(400);
    });

    it('POST /api/v1/agents/:agentId/executions should return 404 for non-existent agent', async () => {
      const res = await request(app)
        .post('/api/v1/agents/non-existent/executions')
        .send({ input: 'Hello' });

      expect(res.status).toBe(404);
    });

    it('GET /api/v1/agents/:agentId/executions should list executions', async () => {
      await request(app)
        .post(`/api/v1/agents/${agentId}/executions`)
        .send({ input: 'Question 1' });

      const res = await request(app).get(
        `/api/v1/agents/${agentId}/executions`,
      );
      expect(res.status).toBe(200);
      expect(res.body.data.length).toBeGreaterThanOrEqual(1);
    });
  });
});
