import { env } from './env';
import { InMemoryAgentRepository } from '../adapters/out/persistence/InMemoryAgentRepository';
import { InMemoryExecutionRepository } from '../adapters/out/persistence/InMemoryExecutionRepository';
import { InMemoryMessagePublisher } from '../adapters/out/messaging/InMemoryMessagePublisher';
import { LLMProviderFactory } from '../adapters/out/llm/LLMProviderFactory';

import { CreateAgentUseCase } from '../../application/use-cases/CreateAgentUseCase';
import { GetAgentUseCase } from '../../application/use-cases/GetAgentUseCase';
import { ListAgentsUseCase } from '../../application/use-cases/ListAgentsUseCase';
import { ExecuteAgentUseCase } from '../../application/use-cases/ExecuteAgentUseCase';
import {
  GetExecutionUseCase,
  ListExecutionsByAgentUseCase,
} from '../../application/use-cases/ExecutionQueryUseCases';

import { AgentController } from '../adapters/in/http/AgentController';
import { ExecutionController } from '../adapters/in/http/ExecutionController';
import { HealthController } from '../adapters/in/http/HealthController';
import { AgentWorker } from '../adapters/in/worker/AgentWorker';

export interface AppContainer {
  agentController: AgentController;
  executionController: ExecutionController;
  healthController: HealthController;
  agentWorker: AgentWorker;
}

export function buildContainer(): AppContainer {
  // Repositories (driven ports)
  const agentRepository = new InMemoryAgentRepository();
  const executionRepository = new InMemoryExecutionRepository();

  // Messaging (driven port)
  const messagePublisher = new InMemoryMessagePublisher();

  // LLM providers (driven ports)
  const llmFactory = LLMProviderFactory.createDefault({
    openaiApiKey: env.OPENAI_API_KEY,
    anthropicApiKey: env.ANTHROPIC_API_KEY,
  });
  const llmProvider = llmFactory.getProvider(env.LLM_PROVIDER);

  // Use cases (application layer)
  const createAgentUseCase = new CreateAgentUseCase(
    agentRepository,
    messagePublisher,
  );
  const getAgentUseCase = new GetAgentUseCase(agentRepository);
  const listAgentsUseCase = new ListAgentsUseCase(agentRepository);
  const executeAgentUseCase = new ExecuteAgentUseCase(
    agentRepository,
    executionRepository,
    llmProvider,
    messagePublisher,
  );
  const getExecutionUseCase = new GetExecutionUseCase(executionRepository);
  const listExecutionsByAgentUseCase = new ListExecutionsByAgentUseCase(
    executionRepository,
  );

  // Driving adapters (HTTP controllers)
  const agentController = new AgentController(
    createAgentUseCase,
    getAgentUseCase,
    listAgentsUseCase,
  );
  const executionController = new ExecutionController(
    executeAgentUseCase,
    getExecutionUseCase,
    listExecutionsByAgentUseCase,
  );
  const healthController = new HealthController();

  // Driving adapter (worker)
  const agentWorker = new AgentWorker(executeAgentUseCase, executionRepository, {
    concurrency: env.WORKER_CONCURRENCY,
    pollIntervalMs: env.WORKER_POLL_INTERVAL_MS,
  });

  return {
    agentController,
    executionController,
    healthController,
    agentWorker,
  };
}
