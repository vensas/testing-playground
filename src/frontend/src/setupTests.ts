// jest-dom adds custom jest matchers for asserting on DOM nodes.
// allows you to do things like:
// expect(element).toHaveTextContent(/react/i)
// learn more: https://github.com/testing-library/jest-dom
import '@testing-library/jest-dom';
import { toHaveNoViolations } from "jest-axe";

// mock fetch
global.fetch = jest.fn();

// Add the a11y matcher to Jest
expect.extend(toHaveNoViolations);