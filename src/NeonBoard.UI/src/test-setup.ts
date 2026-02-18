import 'zone.js';
import 'zone.js/testing';
import { getTestBed } from '@angular/core/testing';
import {
  BrowserTestingModule,
  platformBrowserTesting,
} from '@angular/platform-browser/testing';

let initialized = false;

export function initTestEnvironment(): void {
  if (!initialized) {
    initialized = true;
    getTestBed().initTestEnvironment(
      BrowserTestingModule,
      platformBrowserTesting(),
    );
  }
}
