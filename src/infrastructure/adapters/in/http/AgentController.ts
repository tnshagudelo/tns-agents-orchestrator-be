import { Request, Response, NextFunction } from 'express';
import { ICreateAgentUseCase } from '../../../../application/ports/in/ICreateAgentUseCase';
import { IGetAgentUseCase } from '../../../../application/ports/in/IGetAgentUseCase';
import { IListAgentsUseCase } from '../../../../application/ports/in/IListAgentsUseCase';
import { DomainError } from '../../../../domain/shared/DomainError';
import { z } from 'zod';

const createAgentSchema = z.object({
  name: z.string().min(1),
  description: z.string().min(1),
  systemPrompt: z.string().min(1),
  model: z.string().optional(),
  provider: z.enum(['openai', 'anthropic']).optional(),
  capabilities: z.array(z.string()).optional(),
});

export class AgentController {
  constructor(
    private readonly createAgentUseCase: ICreateAgentUseCase,
    private readonly getAgentUseCase: IGetAgentUseCase,
    private readonly listAgentsUseCase: IListAgentsUseCase,
  ) {}

  list = async (
    _req: Request,
    res: Response,
    next: NextFunction,
  ): Promise<void> => {
    try {
      const agents = await this.listAgentsUseCase.execute();
      res.json({ data: agents.map((a) => a.toJSON()) });
    } catch (error) {
      next(error);
    }
  };

  getById = async (
    req: Request,
    res: Response,
    next: NextFunction,
  ): Promise<void> => {
    try {
      const agent = await this.getAgentUseCase.execute(req.params.id);
      res.json({ data: agent.toJSON() });
    } catch (error) {
      if (error instanceof DomainError && error.code === 'AGENT_NOT_FOUND') {
        res.status(404).json({ error: error.message, code: error.code });
        return;
      }
      next(error);
    }
  };

  create = async (
    req: Request,
    res: Response,
    next: NextFunction,
  ): Promise<void> => {
    try {
      const parsed = createAgentSchema.safeParse(req.body);
      if (!parsed.success) {
        res.status(400).json({
          error: 'Validation failed',
          details: parsed.error.flatten(),
        });
        return;
      }

      const agent = await this.createAgentUseCase.execute(parsed.data);
      res.status(201).json({ data: agent.toJSON() });
    } catch (error) {
      if (error instanceof DomainError) {
        res.status(422).json({ error: error.message, code: error.code });
        return;
      }
      next(error);
    }
  };
}
