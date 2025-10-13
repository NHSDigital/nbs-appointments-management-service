import { logs, SeverityNumber } from '@opentelemetry/api-logs';

const logger = logs.getLogger('mya-web-app');

export function logInfo(message: string, attributes?: Record<string, string>) {
  logger.emit({
    severityNumber: SeverityNumber.INFO,
    severityText: 'INFO',
    body: message,
    attributes: {
      ...attributes,
      'service.name': 'mya-web-app',
    },
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
      'service.name': 'mya-web-app',
    },
    timestamp: Date.now(),
  });
}
