type LogLevel = 'info' | 'warn' | 'error';

export function logEvent(
  level: LogLevel,
  event: string,
  payload: Record<string, any>,
  context = 'App',
) {
  if (process.env.NEXT_PUBLIC_ENV == 'production') return;

  const message = `[${context}] ${event}`;
  const data = JSON.stringify(payload);

  switch (level) {
    case 'info':
      /* eslint-disable no-console */
      console.info(message, data);
      /* eslint-enable no-console */
      break;
    case 'warn':
      /* eslint-disable no-console */
      console.warn(message, data);
      /* eslint-enable no-console */
      break;
    case 'error':
      /* eslint-disable no-console */
      console.error(message, data);
      /* eslint-enable no-console */
      break;
  }
}
