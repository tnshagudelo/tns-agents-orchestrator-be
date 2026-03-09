import { Agent } from '../../../domain/agent/Agent';

export interface IAgentRepository {
  save(agent: Agent): Promise<void>;
  findById(id: string): Promise<Agent | null>;
  findAll(): Promise<Agent[]>;
  delete(id: string): Promise<void>;
}
