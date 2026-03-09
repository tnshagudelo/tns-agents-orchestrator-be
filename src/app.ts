import express, { Application } from 'express';
import { AgentController } from './infrastructure/adapters/in/http/AgentController';
import { ExecutionController } from './infrastructure/adapters/in/http/ExecutionController';
import { HealthController } from './infrastructure/adapters/in/http/HealthController';
import { createAgentRouter } from './infrastructure/adapters/in/http/routers/agentRouter';
import { createExecutionRouter } from './infrastructure/adapters/in/http/routers/executionRouter';
import { createHealthRouter } from './infrastructure/adapters/in/http/routers/healthRouter';
import { errorHandler } from './infrastructure/adapters/in/http/middleware/errorHandler';
import { requestLogger } from './infrastructure/adapters/in/http/middleware/requestLogger';

export function createApp(
  agentController: AgentController,
  executionController: ExecutionController,
  healthController: HealthController,
): Application {
  const app = express();

  app.use(express.json());
  app.use(requestLogger);

  // Routes
  app.use('/health', createHealthRouter(healthController));
  app.use('/api/v1/agents', createAgentRouter(agentController));
  app.use(
    '/api/v1/agents/:agentId/executions',
    createExecutionRouter(executionController),
  );

  // Error handler must be last
  app.use(errorHandler);

  return app;
}
