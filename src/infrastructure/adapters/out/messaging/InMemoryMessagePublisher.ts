import {
  PublishableEvent,
  IMessagePublisher,
} from '../../../../application/ports/out/IMessagePublisher';

type EventHandler = (event: PublishableEvent) => Promise<void>;

export class InMemoryMessagePublisher implements IMessagePublisher {
  private readonly handlers = new Map<string, EventHandler[]>();
  private readonly eventLog: PublishableEvent[] = [];

  async publish(event: PublishableEvent): Promise<void> {
    this.eventLog.push(event);
    const handlers = this.handlers.get(event.type) ?? [];
    await Promise.all(handlers.map((h) => h(event)));
  }

  subscribe(eventType: string, handler: EventHandler): void {
    const existing = this.handlers.get(eventType) ?? [];
    this.handlers.set(eventType, [...existing, handler]);
  }

  /** Useful for testing: returns all published events */
  getEventLog(): PublishableEvent[] {
    return [...this.eventLog];
  }

  /** Useful for testing: clears the event log */
  clearEventLog(): void {
    this.eventLog.length = 0;
  }
}
