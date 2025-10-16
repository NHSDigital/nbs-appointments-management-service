import { metrics } from '@opentelemetry/api';

export type METRICS_METER = 'PAGE_VIEWS' | 'API_CALLS';

// TODO: Populate this with all the uses we need of metrics. Can do histograms, counters etc. just need to decide what we want
export const createCounter = (
  meterName: METRICS_METER,
  counterName: string,
) => {
  const counter = metrics.getMeter(meterName).createCounter(counterName);

  return counter;
};
