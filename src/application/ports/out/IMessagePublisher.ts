export interface PublishableEvent {
  type: string;
  payload: Record<string, unknown>;
  timestamp: Date;
}

export interface IMessagePublisher {
  publish(event: PublishableEvent): Promise<void>;
  subscribe(eventType: string, handler: (event: PublishableEvent) => Promise<void>): void;
}
