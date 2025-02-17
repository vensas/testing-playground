import React, { useState, useEffect, useCallback } from 'react';
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
  Box,
} from '@mui/material';
import { Stack } from '@mui/system';

const BACKEND_URL = process.env.REACT_APP_BACKEND_URL;
const votesUrl = `${BACKEND_URL}/votes`;
const resultsUrl = `${BACKEND_URL}/results`;

export interface Vote {
  candidate: string;
  party: string;
}

export interface VoteResult {
  voteCount: number;
  party: string;
}

function useVotes() {
  const [votes, setVotes] = useState<Vote[] | undefined>([]);

  const fetchVotes = useCallback(async () => {
    const response = await fetch(votesUrl);
    const data = await response.json();
    setVotes(data);
  }, [setVotes]);

  useEffect(() => {
    fetchVotes();
  }, [fetchVotes]);

  return { votes, fetchVotes };
}

const App: React.FC = () => {
  const { votes, fetchVotes } = useVotes();
  const [candidate, setCandidate] = useState('');
  const [party, setParty] = useState('');
  const [resultOpen, setResultOpen] = useState(false);
  const [result, setResult] = useState<VoteResult[] | undefined>([]);

  const addVote = async () => {
    await fetch(votesUrl, {
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
    const response = await fetch(resultsUrl);
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

      <Box sx={{ background: 'white' }}>
        <Typography variant='h1' color='white'>
          Not readable
        </Typography>
      </Box>
    </Container>
  );
};

export default App;
