import React from 'react';
import { act, render } from '@testing-library/react';
import { axe } from 'jest-axe';
import App from './App';

beforeEach(() => {
  jest.spyOn(global, 'fetch').mockImplementation(() =>
    Promise.resolve({
      json: () => Promise.resolve([]),
    } as any)
  );
});

afterEach(() => {
  jest.restoreAllMocks();
});

// Tests for all rules here https://dequeuniversity.com/rules/axe/4.9
test('App should have no accessibility violations', async () => {
  const { container } = await act(() => render(<App />));
  const results = await axe(container, {
    rules: { 'target-size': { enabled: true } },
  });
  expect(results).toHaveNoViolations();
});
