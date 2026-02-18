/// <reference types="vitest" />
import { defineConfig } from 'vitest/config';
import angular from '@analogjs/vite-plugin-angular';

export default defineConfig({
  plugins: [angular()],
  test: {
    globals: true,
    include: ['src/**/*.spec.ts'],
    reporters: ['default'],
    environment: 'jsdom',
  },
});
