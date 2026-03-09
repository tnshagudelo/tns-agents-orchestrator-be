import { Request, Response, NextFunction } from 'express';
import { DomainError } from '../../../../../domain/shared/DomainError';

export function errorHandler(
  error: unknown,
  _req: Request,
  res: Response,
  _next: NextFunction,
): void {
  if (error instanceof DomainError) {
    res.status(422).json({
      error: error.message,
      code: error.code,
    });
    return;
  }

  if (error instanceof Error) {
    console.error('[ErrorHandler]', error.message, error.stack);
    res.status(500).json({ error: 'Internal server error' });
    return;
  }

  console.error('[ErrorHandler] Unknown error:', error);
  res.status(500).json({ error: 'Internal server error' });
}
