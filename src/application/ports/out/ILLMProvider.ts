import { LLMRequest, LLMResponse } from '../../../domain/llm/LLMTypes';

export interface ILLMProvider {
  readonly providerName: string;
  complete(request: LLMRequest): Promise<LLMResponse>;
  supportsModel(model: string): boolean;
}
