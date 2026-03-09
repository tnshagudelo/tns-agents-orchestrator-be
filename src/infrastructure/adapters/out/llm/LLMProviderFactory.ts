import { ILLMProvider } from '../../../../application/ports/out/ILLMProvider';
import { OpenAIAdapter } from './OpenAIAdapter';
import { AnthropicAdapter } from './AnthropicAdapter';
import { LLMProviderError } from '../../../../domain/shared/DomainError';

export type LLMProviderName = 'openai' | 'anthropic';

export class LLMProviderFactory {
  private readonly providers = new Map<string, ILLMProvider>();

  registerProvider(provider: ILLMProvider): void {
    this.providers.set(provider.providerName, provider);
  }

  getProvider(providerName: string): ILLMProvider {
    const provider = this.providers.get(providerName);
    if (!provider) {
      throw new LLMProviderError(
        providerName,
        `Provider '${providerName}' is not registered. Available: [${Array.from(this.providers.keys()).join(', ')}]`,
      );
    }
    return provider;
  }

  getProviderForModel(model: string): ILLMProvider {
    for (const provider of this.providers.values()) {
      if (provider.supportsModel(model)) {
        return provider;
      }
    }
    throw new LLMProviderError(
      'unknown',
      `No registered provider supports model '${model}'`,
    );
  }

  static createDefault(config: {
    openaiApiKey?: string;
    anthropicApiKey?: string;
  }): LLMProviderFactory {
    const factory = new LLMProviderFactory();

    if (config.openaiApiKey) {
      factory.registerProvider(new OpenAIAdapter(config.openaiApiKey));
    }
    if (config.anthropicApiKey) {
      factory.registerProvider(new AnthropicAdapter(config.anthropicApiKey));
    }

    return factory;
  }
}
