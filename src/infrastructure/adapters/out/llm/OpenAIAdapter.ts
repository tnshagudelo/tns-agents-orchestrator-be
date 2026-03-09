import OpenAI from 'openai';
import { ILLMProvider } from '../../../../application/ports/out/ILLMProvider';
import { LLMRequest, LLMResponse } from '../../../../domain/llm/LLMTypes';
import { LLMProviderError } from '../../../../domain/shared/DomainError';

const OPENAI_MODELS = [
  'gpt-4o',
  'gpt-4o-mini',
  'gpt-4-turbo',
  'gpt-4',
  'gpt-3.5-turbo',
];

export class OpenAIAdapter implements ILLMProvider {
  readonly providerName = 'openai';
  private readonly client: OpenAI;

  constructor(apiKey: string) {
    this.client = new OpenAI({ apiKey });
  }

  async complete(request: LLMRequest): Promise<LLMResponse> {
    try {
      const response = await this.client.chat.completions.create({
        model: request.model,
        messages: request.messages.map((m) => ({
          role: m.role as 'system' | 'user' | 'assistant',
          content: m.content,
        })),
        max_tokens: request.maxTokens,
        temperature: request.temperature,
      });

      const choice = response.choices[0];
      if (!choice?.message?.content) {
        throw new LLMProviderError('openai', 'Empty response from API');
      }

      return {
        content: choice.message.content,
        model: response.model,
        provider: this.providerName,
        usage: {
          promptTokens: response.usage?.prompt_tokens ?? 0,
          completionTokens: response.usage?.completion_tokens ?? 0,
          totalTokens: response.usage?.total_tokens ?? 0,
        },
        metadata: { finishReason: choice.finish_reason },
      };
    } catch (error) {
      if (error instanceof LLMProviderError) throw error;
      const message = error instanceof Error ? error.message : String(error);
      throw new LLMProviderError('openai', message);
    }
  }

  supportsModel(model: string): boolean {
    return OPENAI_MODELS.some((m) => model.startsWith(m));
  }
}
