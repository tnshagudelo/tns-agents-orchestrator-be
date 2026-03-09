import { Router } from 'express';
import { HealthController } from '../HealthController';

export function createHealthRouter(controller: HealthController): Router {
  const router = Router();

  router.get('/', controller.check);

  return router;
}
