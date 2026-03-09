import { AgentExecution } from '../../domain/agent/AgentExecution';
import {
  IGetExecutionUseCase,
  IListExecutionsByAgentUseCase,
} from '../ports/in/IExecutionQueryUseCases';
import { IExecutionRepository } from '../ports/out/IExecutionRepository';
import { ExecutionNotFoundError } from '../../domain/shared/DomainError';

export class GetExecutionUseCase implements IGetExecutionUseCase {
  constructor(private readonly executionRepository: IExecutionRepository) {}

  async execute(executionId: string): Promise<AgentExecution> {
    const execution = await this.executionRepository.findById(executionId);
    if (!execution) {
      throw new ExecutionNotFoundError(executionId);
    }
    return execution;
  }
}

export class ListExecutionsByAgentUseCase
  implements IListExecutionsByAgentUseCase
{
  constructor(private readonly executionRepository: IExecutionRepository) {}

  async execute(agentId: string): Promise<AgentExecution[]> {
    return this.executionRepository.findByAgentId(agentId);
  }
}
