import React from 'react';
import {
  act,
  render,
  screen,
  fireEvent,
  waitFor,
} from '@testing-library/react';
import App, { Vote, VoteResult } from './App';
import '@testing-library/jest-dom';

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

test('renders vote list', async () => {
  jest.spyOn(global, 'fetch').mockImplementation(() =>
    Promise.resolve({
      json: () =>
        Promise.resolve([
          { party: 'Lonely Mountain', candidate: 'Gimli' },
          { party: 'Gondor', candidate: 'Boromir' },
        ] as Vote[]),
    } as any)
  );

  render(<App />);

  await waitFor(() => {
    expect(screen.getByText(/Gimli/i)).toBeInTheDocument();
    expect(screen.getByText(/Lonely Mountain/i)).toBeInTheDocument();
    expect(screen.getByText(/Boromir/i)).toBeInTheDocument();
    expect(screen.getByText(/Gondor/i)).toBeInTheDocument();
  });
});

test('adds a vote', async () => {
  render(<App />);

  act(() => {
    fireEvent.change(screen.getByLabelText(/Candidate/i), {
      target: { value: 'Gimli' },
    });
    fireEvent.change(screen.getByLabelText(/Party/i), {
      target: { value: 'Lonely Mountain' },
    });
    fireEvent.click(screen.getByTestId('add-vote'));
  });

  await waitFor(() => {
    expect(fetch as jest.Mock).toBeCalledWith(
      expect.stringContaining('/votes'),
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ candidate: 'Gimli', party: 'Lonely Mountain' }),
      }
    );
  });
});

test('shows results', async () => {
  jest.spyOn(global, 'fetch').mockImplementation((url) => {
    if (typeof url === 'string' && url.includes('/results')) {
      return Promise.resolve({
        json: async () =>
          [
            { party: 'Lonely Mountain', voteCount: 2 },
            { party: 'Gondor', voteCount: 1 },
          ] as VoteResult[],
      } as unknown as Response);
    }
    // For any other fetch call (like the initial votes fetch)
    return Promise.resolve({
      json: async () =>
        [
          { party: 'Lonely Mountain', candidate: 'Gimli' },
          { party: 'Gondor', candidate: 'Boromir' },
          { party: 'Lonely Mountain', candidate: 'Gimli' },
        ] as Vote[],
    } as unknown as Response);
  });

  render(<App />);
  await waitFor(() => {
    expect(screen.getAllByText(/Lonely Mountain/i)).toHaveLength(2);
  });

  act(() => {
    fireEvent.click(screen.getByTestId('calculate'));
  });

  await waitFor(() => {
    expect(screen.getByText(/Lonely Mountain \(2\)/i)).toBeInTheDocument();
    expect(screen.getByText(/Gondor \(1\)/i)).toBeInTheDocument();
  });
});
