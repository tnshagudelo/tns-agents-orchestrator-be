import Anthropic from '@anthropic-ai/sdk';
import { ILLMProvider } from '../../../../application/ports/out/ILLMProvider';
import { LLMRequest, LLMResponse, MessageRole } from '../../../../domain/llm/LLMTypes';
import { LLMProviderError } from '../../../../domain/shared/DomainError';

const ANTHROPIC_MODELS = [
  'claude-3-5-sonnet',
  'claude-3-5-haiku',
  'claude-3-opus',
  'claude-3-sonnet',
  'claude-3-haiku',
];

export class AnthropicAdapter implements ILLMProvider {
  readonly providerName = 'anthropic';
  private readonly client: Anthropic;

  constructor(apiKey: string) {
    this.client = new Anthropic({ apiKey });
  }

  async complete(request: LLMRequest): Promise<LLMResponse> {
    try {
      // Anthropic separates system prompt from messages
      const systemMessage = request.messages.find(
        (m) => m.role === MessageRole.SYSTEM,
      );
      const userMessages = request.messages
        .filter((m) => m.role !== MessageRole.SYSTEM)
        .map((m) => ({
          role: m.role as 'user' | 'assistant',
          content: m.content,
        }));

      const response = await this.client.messages.create({
        model: request.model,
        system: systemMessage?.content,
        messages: userMessages,
        max_tokens: request.maxTokens ?? 1024,
      });

      const firstBlock = response.content[0];
      if (!firstBlock || firstBlock.type !== 'text') {
        throw new LLMProviderError('anthropic', 'Empty response from API');
      }

      return {
        content: firstBlock.text,
        model: response.model,
        provider: this.providerName,
        usage: {
          promptTokens: response.usage.input_tokens,
          completionTokens: response.usage.output_tokens,
          totalTokens:
            response.usage.input_tokens + response.usage.output_tokens,
        },
        metadata: { stopReason: response.stop_reason },
      };
    } catch (error) {
      if (error instanceof LLMProviderError) throw error;
      const message = error instanceof Error ? error.message : String(error);
      throw new LLMProviderError('anthropic', message);
    }
  }

  supportsModel(model: string): boolean {
    return ANTHROPIC_MODELS.some((m) => model.startsWith(m));
  }
}
