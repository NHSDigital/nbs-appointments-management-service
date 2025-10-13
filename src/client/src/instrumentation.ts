import { registerOTel } from '@vercel/otel';
import { OTLPLogExporter } from '@opentelemetry/exporter-logs-otlp-http';
import {
  SimpleLogRecordProcessor,
  LoggerProvider,
} from '@opentelemetry/sdk-logs';
import { OTLPMetricExporter } from '@opentelemetry/exporter-metrics-otlp-http';
import {
  MeterProvider,
  PeriodicExportingMetricReader,
} from '@opentelemetry/sdk-metrics';
import { logs } from '@opentelemetry/api-logs';
import { metrics } from '@opentelemetry/api';

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
  // We send metrics to the otel collector using open telemetry's node SDK
  const splunkMetricExporter = new OTLPMetricExporter({
    url: process.env.OTEL_EXPORTER_OTLP_METRICS_ENDPOINT,
    headers: {
      Authorization: splunkAuthHeader,
    },
  });
  const splunkMetricReader = new PeriodicExportingMetricReader({
    exporter: splunkMetricExporter,
    exportIntervalMillis: 1000,
  });

  // NextJS exports request traces out of the box
  const meterProvider = new MeterProvider({
    readers: [splunkMetricReader],
  });

  metrics.setGlobalMeterProvider(meterProvider);
}
