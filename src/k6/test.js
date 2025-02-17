import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';

const BASE_URL = 'http://localhost:5266';
const votesTrend = new Trend('votes');

export let options = {
    stages: [
        { duration: '30s', target: 100 }, // ramp up to 100 users
        { duration: '1m', target: 100 }, // stay at 100 users
        { duration: '30s', target: 0 }, // ramp down to 0 users
    ],
};

const characters = [
    { Candidate: 'Frodo Baggins', Party: 'Shire' },
    { Candidate: 'Aragorn', Party: 'Rivendale' },
    { Candidate: 'Legolas', Party: 'Wood Elves' },
    { Candidate: 'Gimli', Party: 'Lonely Mountain' },
];

export default function () {
    // List all votes
    let res = http.get(`${BASE_URL}/votes`);
    check(res, { 'list votes status was 200': (r) => r.status === 200 });
    votesTrend.add(res.timings.duration);

    // Create a vote
    const vote = characters[Math.floor(Math.random() * characters.length)];
    res = http.post(`${BASE_URL}/votes`, JSON.stringify(vote), {
        headers: { 'Content-Type': 'application/json' },
    });
    check(res, { 'create vote status was 201': (r) => r.status === 201 });
    votesTrend.add(res.timings.duration);

    // List all votes again
    res = http.get(`${BASE_URL}/votes`);
    check(res, { 'list votes status was 200': (r) => r.status === 200 });
    votesTrend.add(res.timings.duration);

    // List results
    res = http.get(`${BASE_URL}/results`);
    check(res, { 'list results status was 200': (r) => r.status === 200 });
    votesTrend.add(res.timings.duration);

    sleep(1);
}
