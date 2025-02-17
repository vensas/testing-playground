import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';
test.describe('App-a11y', () => {
  test('should not have any automatically detectable accessibility issues', async ({
    page,
  }) => {
    const accessibilityScanResults = await new AxeBuilder({ page }).analyze();

    expect(accessibilityScanResults.violations).toEqual([]);
  });
});
