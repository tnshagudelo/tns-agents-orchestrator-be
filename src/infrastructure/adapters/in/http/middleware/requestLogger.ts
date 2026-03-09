import { Request, Response, NextFunction } from 'express';

export function requestLogger(
  req: Request,
  _res: Response,
  next: NextFunction,
): void {
  console.info(`[${new Date().toISOString()}] ${req.method} ${req.url}`);
  next();
}
