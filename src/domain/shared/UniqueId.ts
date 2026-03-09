import { v4 as uuidv4 } from 'uuid';

export class UniqueId {
  private readonly _value: string;

  constructor(value?: string) {
    this._value = value ?? uuidv4();
  }

  get value(): string {
    return this._value;
  }

  equals(other: UniqueId): boolean {
    return this._value === other._value;
  }

  toString(): string {
    return this._value;
  }
}
