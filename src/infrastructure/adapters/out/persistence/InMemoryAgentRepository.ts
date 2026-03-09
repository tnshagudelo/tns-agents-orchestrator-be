import { Agent } from '../../../../domain/agent/Agent';
import { IAgentRepository } from '../../../../application/ports/out/IAgentRepository';

export class InMemoryAgentRepository implements IAgentRepository {
  private readonly store = new Map<string, Agent>();

  async save(agent: Agent): Promise<void> {
    this.store.set(agent.id, agent);
  }

  async findById(id: string): Promise<Agent | null> {
    return this.store.get(id) ?? null;
  }

  async findAll(): Promise<Agent[]> {
    return Array.from(this.store.values());
  }

  async delete(id: string): Promise<void> {
    this.store.delete(id);
  }
}
