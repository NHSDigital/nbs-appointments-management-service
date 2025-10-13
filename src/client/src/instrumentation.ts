import { registerOTel } from '@vercel/otel';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-http';
import {
  SimpleLogRecordProcessor,
  LoggerProvider,
} from '@opentelemetry/sdk-logs';
import { logs } from '@opentelemetry/api-logs';

export function register() {
  // Set up traces.
  // NextJS exports request traces out of the box
  registerOTel({ serviceName: 'mya-web-app' });

  // Set up logging.
  // We send logs to the otel collector using open telemetry's node SDK
  const splunkAuthHeader = `Splunk ${process.env.SPLUNK_HEC_TOKEN}`;
  const splunkLogExporter = new OTLPLogExporter({
    url: process.env.OTEL_EXPORTER_OTLP_LOGS_ENDPOINT,
    headers: {
      Authorization: splunkAuthHeader,
    },
  });

  const loggerProvider = new LoggerProvider({
    logRecordLimits: {
      attributeValueLengthLimit: 8192,
      attributeCountLimit: 128,
    },
    processors: [new SimpleLogRecordProcessor(splunkLogExporter)],
  });

  logs.setGlobalLoggerProvider(loggerProvider);

  // Set up metrics.
  // TODO
}
