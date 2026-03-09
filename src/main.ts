import { createApp } from './app';
import { buildContainer } from './infrastructure/config/container';
import { env } from './infrastructure/config/env';

async function bootstrap(): Promise<void> {
  const container = buildContainer();
  const app = createApp(
    container.agentController,
    container.executionController,
    container.healthController,
  );

  const server = app.listen(env.PORT, () => {
    console.info(
      `[Server] tns-agents-orchestrator-be running on port ${env.PORT} (${env.NODE_ENV})`,
    );
  });

  container.agentWorker.start();

  const shutdown = () => {
    console.info('[Server] Shutting down...');
    container.agentWorker.stop();
    server.close(() => {
      console.info('[Server] Closed.');
      process.exit(0);
    });
  };

  process.on('SIGTERM', shutdown);
  process.on('SIGINT', shutdown);
}

bootstrap().catch((error) => {
  console.error('[Bootstrap] Fatal error:', error);
  process.exit(1);
});
