/* eslint-disable testing-library/no-unnecessary-act */
/* eslint-disable testing-library/no-wait-for-multiple-assertions */
import {
  render,
  screen,
  fireEvent,
  waitFor,
  act,
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
    expect(screen.getByText(/Votes List/i)).toBeInTheDocument();
    expect(screen.getByText(/Gimli/i)).toBeInTheDocument();
    expect(screen.getByText(/Lonely Mountain/i)).toBeInTheDocument();
    expect(screen.getByText(/Boromir/i)).toBeInTheDocument();
    expect(screen.getByText(/Gondor/i)).toBeInTheDocument();
  });
});

test('adds a vote', async () => {
  render(<App />);

  fireEvent.change(screen.getByLabelText(/Candidate/i), {
    target: { value: 'Gimli' },
  });
  fireEvent.change(screen.getByLabelText(/Party/i), {
    target: { value: 'Lonely Mountain' },
  });

  await act(async () => {
    fireEvent.click(screen.getByText(/Add Vote/i));
  });

  await waitFor(() => {
    expect(fetch as jest.Mock).toBeCalledWith('/votes', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ candidate: 'Gimli', party: 'Lonely Mountain' }),
    });
  });
});

test('shows results', async () => {
  jest.spyOn(global, 'fetch').mockImplementation(() =>
    Promise.resolve({
      json: () =>
        Promise.resolve([
          { party: 'Lonely Mountain', voteCount: '2' },
          { party: 'Gondor', voteCount: '1' },
        ] as unknown as VoteResult[]),
    } as any)
  );

  await act(async () => {
    render(<App />);
  });

  await act(async () => {
    fireEvent.click(screen.getByText(/Calculate Result/i));
  });

  expect(screen.getByText(/Lonely Mountain: 2/i)).toBeInTheDocument();
  expect(screen.getByText(/Gondor: 1/i)).toBeInTheDocument();
});
