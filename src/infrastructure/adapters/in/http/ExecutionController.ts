import { Request, Response, NextFunction } from 'express';
import { IExecuteAgentUseCase } from '../../../../application/ports/in/IExecuteAgentUseCase';
import {
  IGetExecutionUseCase,
  IListExecutionsByAgentUseCase,
} from '../../../../application/ports/in/IExecutionQueryUseCases';
import { DomainError } from '../../../../domain/shared/DomainError';
import { z } from 'zod';

const executeAgentSchema = z.object({
  input: z.string().min(1),
  metadata: z.record(z.unknown()).optional(),
});

export class ExecutionController {
  constructor(
    private readonly executeAgentUseCase: IExecuteAgentUseCase,
    private readonly getExecutionUseCase: IGetExecutionUseCase,
    private readonly listExecutionsByAgentUseCase: IListExecutionsByAgentUseCase,
  ) {}

  executeAgent = async (
    req: Request,
    res: Response,
    next: NextFunction,
  ): Promise<void> => {
    try {
      const parsed = executeAgentSchema.safeParse(req.body);
      if (!parsed.success) {
        res.status(400).json({
          error: 'Validation failed',
          details: parsed.error.flatten(),
        });
        return;
      }

      const execution = await this.executeAgentUseCase.execute({
        agentId: req.params.agentId,
        input: parsed.data.input,
        metadata: parsed.data.metadata,
      });

      res.status(201).json({ data: execution.toJSON() });
    } catch (error) {
      if (error instanceof DomainError && error.code === 'AGENT_NOT_FOUND') {
        res.status(404).json({ error: error.message, code: error.code });
        return;
      }
      next(error);
    }
  };

  getById = async (
    req: Request,
    res: Response,
    next: NextFunction,
  ): Promise<void> => {
    try {
      const execution = await this.getExecutionUseCase.execute(
        req.params.executionId,
      );
      res.json({ data: execution.toJSON() });
    } catch (error) {
      if (
        error instanceof DomainError &&
        error.code === 'EXECUTION_NOT_FOUND'
      ) {
        res.status(404).json({ error: error.message, code: error.code });
        return;
      }
      next(error);
    }
  };

  listByAgent = async (
    req: Request,
    res: Response,
    next: NextFunction,
  ): Promise<void> => {
    try {
      const executions = await this.listExecutionsByAgentUseCase.execute(
        req.params.agentId,
      );
      res.json({ data: executions.map((e) => e.toJSON()) });
    } catch (error) {
      next(error);
    }
  };
}
