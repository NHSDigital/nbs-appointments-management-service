import { initializeAppInsights, getAppInsightsClient } from './appInsights';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';

jest.mock('@microsoft/applicationinsights-web');

describe('AppInsights module', () => {
  const mockLoadAppInsights = jest.fn();

  beforeEach(() => {
    jest.clearAllMocks();
    (ApplicationInsights as jest.Mock).mockImplementation(() => ({
      loadAppInsights: mockLoadAppInsights,
    }));
  });

  test('initializeAppInsights initializes ApplicationInsights only once', () => {
    const connectionString = 'test-connection-string';

    const client1 = initializeAppInsights(connectionString);
    const client2 = initializeAppInsights(connectionString);

    expect(ApplicationInsights).toHaveBeenCalledTimes(1);
    expect(mockLoadAppInsights).toHaveBeenCalledTimes(1);
    expect(client1).toBe(client2);
  });

  test('getAppInsightsClient returns initialized client', () => {
    const connectionString = 'test-connection-string';
    const initializedClient = initializeAppInsights(connectionString);

    const client = getAppInsightsClient();
    expect(client).toBe(initializedClient);
  });
});
