import { test, expect } from '@playwright/test';

test.describe('App', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:3000');
  });

  test('should display the title', async ({ page }) => {
    await expect(page.locator('h3')).toHaveText('Votes');
  });

  test('should add a vote', async ({ page }) => {
    // get the inner candidate input and fill it with 'Gimli'
    await page.getByTestId('candidate').locator('input').fill('Gimli');
    await page.getByTestId('party').locator('input').fill('Lonely Mountain');
    await page.getByTestId('add-vote').click();

    const votes = page.locator('ul > li');
    await expect(votes.first()).toContainText('Gimli (Lonely Mountain)');
  });

  test('should calculate result', async ({ page }) => {
    await page.getByTestId('candidate').locator('input').fill('Gimli');
    await page.getByTestId('party').locator('input').fill('Lonely Mountain');
    await page.getByTestId('add-vote').click();

    await page.getByTestId('calculate').click();
    const dialog = page.locator('div[role="dialog"]');
    await expect(dialog).toBeVisible();
    await expect(dialog).toContainText('Lonely Mountain');
  });

  test('should disable add vote button when inputs are empty', async ({
    page,
  }) => {
    const addVoteButton = await page.getByTestId('add-vote');
    await expect(addVoteButton).toBeDisabled();
  });

  test('should enable add vote button when inputs are filled', async ({
    page,
  }) => {
    await page.getByTestId('candidate').locator('input').fill('Gimli');
    await page.getByTestId('party').locator('input').fill('Lonely Mountain');
    const addVoteButton = await page.getByTestId('add-vote');
    await expect(addVoteButton).toBeEnabled();
  });

});
