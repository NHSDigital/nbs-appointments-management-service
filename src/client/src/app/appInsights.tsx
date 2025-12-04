'use client';

import { ApplicationInsights } from '@microsoft/applicationinsights-web';

let appInsightsClient: ApplicationInsights | null = null;

export function initializeAppInsights(connectionString: string) {
  if (!appInsightsClient) {
    appInsightsClient = new ApplicationInsights({
      config: { connectionString },
    });
    appInsightsClient.loadAppInsights();
  }

  return appInsightsClient;
}

export function getAppInsightsClient() {
  if (!appInsightsClient) {
    throw new Error(
      'AppInsights has not been initialised. Call <AppInsightsInitializer /> in the layout first.',
    );
  }
  return appInsightsClient;
}
