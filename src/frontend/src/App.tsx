import React, { useState, useEffect } from 'react';
import {
  Container,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  TextField,
  List,
  ListItem,
  ListItemText,
  Typography,
} from '@mui/material';
import { Stack } from '@mui/system';

export interface Vote {
  candidate: string;
  party: string;
}

export interface VoteResult {
  voteCount: number;
  party: string;
}

const App: React.FC = () => {
  const [votes, setVotes] = useState<Vote[] | undefined>([]);
  const [candidate, setCandidate] = useState('');
  const [party, setParty] = useState('');
  const [resultOpen, setResultOpen] = useState(false);
  const [result, setResult] = useState<VoteResult[] | undefined>([]);

  useEffect(() => {
    fetchVotes();
  }, []);

  const fetchVotes = async () => {
    const response = await fetch('/votes');
    const data = await response.json();
    setVotes(data);
  };

  const addVote = async () => {
    await fetch('/votes', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ candidate, party }),
    });
    await fetchVotes();
    setCandidate('');
    setParty('');
  };

  const calculateResult = async () => {
    const response = await fetch('/results');
    const data = await response.json();
    setResult(data);
    setResultOpen(true);
  };

  return (
    <Container>
      <Typography variant='h1'>Votes List</Typography>
      <Stack spacing={2} direction='row'>
        <TextField
          label='Candidate'
          value={candidate}
          onChange={(e) => setCandidate(e.target.value)}
        />
        <TextField
          label='Party'
          value={party}
          onChange={(e) => setParty(e.target.value)}
        />
        <Button onClick={addVote}>Add Vote</Button>
        <Button onClick={calculateResult}>Calculate Result</Button>
      </Stack>
      {votes?.length === 0 && (
        <Typography variant='caption'>No votes</Typography>
      )}
      {!!votes?.length && (
        <List>
          {votes?.map((vote, index) => (
            <ListItem key={index}>
              <ListItemText primary={`${vote.candidate} (${vote.party})`} />
            </ListItem>
          ))}
        </List>
      )}
      <Dialog open={resultOpen} onClose={() => setResultOpen(false)}>
        <DialogTitle>Vote Result</DialogTitle>
        <DialogContent>
          {!!result?.length ? (
            <List>
              {result != null &&
                result.map((r) => (
                  <ListItem key={r.party}>
                    {r.party}: {r.voteCount}
                  </ListItem>
                ))}
            </List>
          ) : (
            <Typography variant='caption'>No Result!</Typography>
          )}
        </DialogContent>
      </Dialog>
    </Container>
  );
};

export default App;
