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
  ListItemAvatar,
  Paper,
} from '@mui/material';
import { Stack } from '@mui/system';
import { CheckCircle, HowToVote, ThumbUpAltOutlined } from '@mui/icons-material';

// Load BACKEND_URL from environment variables
const BACKEND_URL = process.env.BACKEND_URL;

const votesUrl = encodeURI(`${BACKEND_URL}/votes`);
const resultsUrl = encodeURI(`${BACKEND_URL}/results`);

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
      <Typography variant='h1'>Votes</Typography>
      <Paper
        sx={(theme) => ({ mt: theme.spacing(4), padding: theme.spacing(2) })}
      >
        <Typography variant='h4'>Add Vote</Typography>
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
          <Button
            variant='outlined'
            disabled={!candidate?.length || !party?.length}
            onClick={addVote}
          >
            Add Vote
          </Button>
          <Button variant='contained' onClick={calculateResult}>
            Calculate Result
          </Button>
        </Stack>
      </Paper>
      <Paper
        sx={(theme) => ({ mt: theme.spacing(4), padding: theme.spacing(2) })}
      >
        <Typography variant='h4'>All Votes</Typography>
        <Typography variant='caption'>
          Candidate (Party). Total : {votes?.length}
        </Typography>
        {votes?.length === 0 && (
          <Typography variant='caption'>No votes</Typography>
        )}
        {!!votes?.length && (
          <List>
            {votes?.map((vote, index) => (
              <ListItem key={index}>
                <ListItemAvatar>
                  <ThumbUpAltOutlined />
                </ListItemAvatar>
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
                  result.map((result,index) => (
                    <ListItem key={result.party} >
                       <ListItemAvatar>
                        {index === 0 && <CheckCircle color="success" />}
                       {index > 0 &&  <HowToVote /> }
                      </ListItemAvatar>
                      <ListItemText primary={`${result.party} (${result.voteCount})`} />
                    </ListItem>
                  ))}
              </List>
            ) : (
              <Typography variant='caption'>No Result!</Typography>
            )}
          </DialogContent>
        </Dialog>
      </Paper>
    </Container>
  );
};

export default App;
