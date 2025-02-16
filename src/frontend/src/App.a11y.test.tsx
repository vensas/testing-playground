/* eslint-disable testing-library/no-unnecessary-act */
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

test('App should have no accessibility violations', async () => {
  const { container } = await act(() => render(<App />));
  const results = await axe(container);
  expect(results).toHaveNoViolations();
});
