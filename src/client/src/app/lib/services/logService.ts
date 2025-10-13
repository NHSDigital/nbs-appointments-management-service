import { logs, SeverityNumber } from '@opentelemetry/api-logs';

const logger = logs.getLogger('default');

export function logInfo(message: string, attributes?: Record<string, string>) {
  logger.emit({
    severityNumber: SeverityNumber.INFO,
    severityText: 'INFO',
    body: message,
    attributes,
    timestamp: Date.now(),
  });
}

export function logError(
  message: string,
  error?: Error,
  attributes?: Record<string, string>,
) {
  logger.emit({
    severityNumber: SeverityNumber.ERROR,
    severityText: 'ERROR',
    body: message,
    attributes: {
      ...attributes,
      'error.message': error?.message,
      'error.stack': error?.stack,
    },
    timestamp: Date.now(),
  });
}
