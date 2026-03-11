import { generateKeyBetween } from 'fractional-indexing';

export function getPositionBetween(before: string | null, after: string | null): string {
  return generateKeyBetween(before, after);
}
