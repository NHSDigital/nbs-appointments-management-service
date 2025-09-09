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
      console.info(message, data);
      break;
    case 'warn':
      console.warn(message, data);
      break;
    case 'error':
      console.error(message, data);
      break;
  }
}
