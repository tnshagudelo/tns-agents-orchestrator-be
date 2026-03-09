import { IExecuteAgentUseCase } from '../../../../application/ports/in/IExecuteAgentUseCase';
import { IExecutionRepository } from '../../../../application/ports/out/IExecutionRepository';
import { ExecutionStatus } from '../../../../domain/agent/AgentExecution';

export interface AgentWorkerConfig {
  concurrency?: number;
  pollIntervalMs?: number;
}

/**
 * AgentWorker is a background worker that polls for pending executions
 * and processes them concurrently up to the configured concurrency limit.
 *
 * This is the "driving" worker adapter that acts as the entry point
 * for background processing in the hexagonal architecture.
 */
export class AgentWorker {
  private running = false;
  private timer: ReturnType<typeof setTimeout> | null = null;
  private readonly concurrency: number;
  private readonly pollIntervalMs: number;

  constructor(
    private readonly executeAgentUseCase: IExecuteAgentUseCase,
    private readonly executionRepository: IExecutionRepository,
    config: AgentWorkerConfig = {},
  ) {
    this.concurrency = config.concurrency ?? 5;
    this.pollIntervalMs = config.pollIntervalMs ?? 1000;
  }

  start(): void {
    if (this.running) return;
    this.running = true;
    console.info('[AgentWorker] Starting...');
    this.scheduleNextPoll();
  }

  stop(): void {
    this.running = false;
    if (this.timer) {
      clearTimeout(this.timer);
      this.timer = null;
    }
    console.info('[AgentWorker] Stopped.');
  }

  private scheduleNextPoll(): void {
    if (!this.running) return;
    this.timer = setTimeout(() => this.poll(), this.pollIntervalMs);
  }

  private async poll(): Promise<void> {
    try {
      const pending = await this.executionRepository.findPending();

      const toProcess = pending
        .filter((e) => e.status === ExecutionStatus.PENDING)
        .slice(0, this.concurrency);

      if (toProcess.length > 0) {
        console.info(
          `[AgentWorker] Processing ${toProcess.length} pending execution(s)`,
        );
        await Promise.allSettled(
          toProcess.map((execution) =>
            this.executeAgentUseCase
              .execute({
                agentId: execution.agentId,
                input: execution.input,
                metadata: execution.metadata,
              })
              .catch((err: unknown) => {
                const msg =
                  err instanceof Error ? err.message : String(err);
                console.error(
                  `[AgentWorker] Failed execution ${execution.id}: ${msg}`,
                );
              }),
          ),
        );
      }
    } catch (error) {
      console.error('[AgentWorker] Poll error:', error);
    } finally {
      this.scheduleNextPoll();
    }
  }
}
