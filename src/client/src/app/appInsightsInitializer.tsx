'use client';

import { useEffect } from 'react';
import { initializeAppInsights } from './appInsights';

interface Props {
  connectionString: string;
}

export function AppInsightsInitializer({ connectionString }: Props) {
  useEffect(() => {
    const ai = initializeAppInsights(connectionString);
    ai.trackPageView();
  }, [connectionString]);

  return null;
}
