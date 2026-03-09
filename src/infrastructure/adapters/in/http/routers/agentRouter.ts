import { Router } from 'express';
import { AgentController } from '../AgentController';

export function createAgentRouter(controller: AgentController): Router {
  const router = Router();

  router.get('/', controller.list);
  router.get('/:id', controller.getById);
  router.post('/', controller.create);

  return router;
}
