import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { formatRelativeTime, RelativeTimePipe } from './relative-time.pipe';

describe('formatRelativeTime', () => {
  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-03-01T12:00:00Z'));
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  describe('long format (default)', () => {
    it('should return "Just now" for less than 1 minute ago', () => {
      const date = new Date('2026-03-01T11:59:30Z');
      expect(formatRelativeTime(date)).toBe('Just now');
    });

    it('should return "1 minute ago" for exactly 1 minute', () => {
      const date = new Date('2026-03-01T11:59:00Z');
      expect(formatRelativeTime(date)).toBe('1 minute ago');
    });

    it('should return "5 minutes ago" with pluralization', () => {
      const date = new Date('2026-03-01T11:55:00Z');
      expect(formatRelativeTime(date)).toBe('5 minutes ago');
    });

    it('should return "1 hour ago" for exactly 1 hour', () => {
      const date = new Date('2026-03-01T11:00:00Z');
      expect(formatRelativeTime(date)).toBe('1 hour ago');
    });

    it('should return "3 hours ago" with pluralization', () => {
      const date = new Date('2026-03-01T09:00:00Z');
      expect(formatRelativeTime(date)).toBe('3 hours ago');
    });

    it('should return "1 day ago" for exactly 1 day', () => {
      const date = new Date('2026-02-28T12:00:00Z');
      expect(formatRelativeTime(date)).toBe('1 day ago');
    });

    it('should return "5 days ago" for multiple days', () => {
      const date = new Date('2026-02-24T12:00:00Z');
      expect(formatRelativeTime(date)).toBe('5 days ago');
    });

    it('should return localized date for 30+ days', () => {
      const date = new Date('2026-01-15T12:00:00Z');
      expect(formatRelativeTime(date)).toBe(date.toLocaleDateString());
    });
  });

  describe('short format', () => {
    it('should return "just now" for less than 1 minute ago', () => {
      const date = new Date('2026-03-01T11:59:30Z');
      expect(formatRelativeTime(date, 'short')).toBe('just now');
    });

    it('should return "5m ago" for minutes', () => {
      const date = new Date('2026-03-01T11:55:00Z');
      expect(formatRelativeTime(date, 'short')).toBe('5m ago');
    });

    it('should return "2h ago" for hours', () => {
      const date = new Date('2026-03-01T10:00:00Z');
      expect(formatRelativeTime(date, 'short')).toBe('2h ago');
    });

    it('should return "3d ago" for days', () => {
      const date = new Date('2026-02-26T12:00:00Z');
      expect(formatRelativeTime(date, 'short')).toBe('3d ago');
    });
  });

  describe('input types', () => {
    it('should accept string input', () => {
      expect(formatRelativeTime('2026-03-01T11:59:30Z')).toBe('Just now');
    });

    it('should accept Date input', () => {
      expect(formatRelativeTime(new Date('2026-03-01T11:59:30Z'))).toBe('Just now');
    });
  });
});

describe('RelativeTimePipe', () => {
  let pipe: RelativeTimePipe;

  beforeEach(() => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date('2026-03-01T12:00:00Z'));
    pipe = new RelativeTimePipe();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should return empty string for null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('should return empty string for undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });

  it('should use long format by default', () => {
    expect(pipe.transform('2026-03-01T11:55:00Z')).toBe('5 minutes ago');
  });

  it('should use short format when specified', () => {
    expect(pipe.transform('2026-03-01T11:55:00Z', 'short')).toBe('5m ago');
  });
});
