import { Agent } from '../../../domain/agent/Agent';

export interface IListAgentsUseCase {
  execute(): Promise<Agent[]>;
}
