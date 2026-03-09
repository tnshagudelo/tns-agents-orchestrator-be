import { AgentExecution, ExecutionStatus } from '../../../../domain/agent/AgentExecution';
import { IExecutionRepository } from '../../../../application/ports/out/IExecutionRepository';

export class InMemoryExecutionRepository implements IExecutionRepository {
  private readonly store = new Map<string, AgentExecution>();

  async save(execution: AgentExecution): Promise<void> {
    this.store.set(execution.id, execution);
  }

  async findById(id: string): Promise<AgentExecution | null> {
    return this.store.get(id) ?? null;
  }

  async findByAgentId(agentId: string): Promise<AgentExecution[]> {
    return Array.from(this.store.values()).filter(
      (e) => e.agentId === agentId,
    );
  }

  async findPending(): Promise<AgentExecution[]> {
    return Array.from(this.store.values()).filter(
      (e) => e.status === ExecutionStatus.PENDING,
    );
  }
}
