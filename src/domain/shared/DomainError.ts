export class DomainError extends Error {
  constructor(
    message: string,
    public readonly code: string,
  ) {
    super(message);
    this.name = 'DomainError';
    Object.setPrototypeOf(this, new.target.prototype);
  }
}

export class AgentNotFoundError extends DomainError {
  constructor(agentId: string) {
    super(`Agent with id '${agentId}' was not found`, 'AGENT_NOT_FOUND');
    this.name = 'AgentNotFoundError';
  }
}

export class ExecutionNotFoundError extends DomainError {
  constructor(executionId: string) {
    super(
      `Execution with id '${executionId}' was not found`,
      'EXECUTION_NOT_FOUND',
    );
    this.name = 'ExecutionNotFoundError';
  }
}

export class InvalidAgentError extends DomainError {
  constructor(reason: string) {
    super(`Invalid agent: ${reason}`, 'INVALID_AGENT');
    this.name = 'InvalidAgentError';
  }
}

export class LLMProviderError extends DomainError {
  constructor(provider: string, reason: string) {
    super(`LLM provider '${provider}' error: ${reason}`, 'LLM_PROVIDER_ERROR');
    this.name = 'LLMProviderError';
  }
}
