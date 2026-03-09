import { Router } from 'express';
import { ExecutionController } from '../ExecutionController';

export function createExecutionRouter(controller: ExecutionController): Router {
  const router = Router({ mergeParams: true });

  // POST /agents/:agentId/executions
  router.post('/', controller.executeAgent);
  // GET  /agents/:agentId/executions
  router.get('/', controller.listByAgent);
  // GET  /executions/:executionId
  router.get('/:executionId', controller.getById);

  return router;
}
