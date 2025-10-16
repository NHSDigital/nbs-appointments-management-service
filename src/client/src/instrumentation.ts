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
  registerTraces();
  registerLogs();
  registerMetrics();
}

const registerTraces = () => {
  registerOTel({ serviceName: 'mya-web-app' });
};

const registerLogs = () => {
  const splunkLogExporter = new OTLPLogExporter({
    url: process.env.OTEL_EXPORTER_OTLP_LOGS_ENDPOINT,
    headers: {
      Authorization: `Splunk ${process.env.SPLUNK_HEC_TOKEN}`,
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
};

const registerMetrics = () => {
  const splunkMetricExporter = new OTLPMetricExporter({
    url: process.env.OTEL_EXPORTER_OTLP_METRICS_ENDPOINT,
    headers: {
      Authorization: `Splunk ${process.env.SPLUNK_HEC_TOKEN}`,
    },
  });
  const splunkMetricReader = new PeriodicExportingMetricReader({
    exporter: splunkMetricExporter,
    exportIntervalMillis: 10000,
  });

  // NextJS exports request traces out of the box
  const meterProvider = new MeterProvider({
    readers: [splunkMetricReader],
  });

  metrics.setGlobalMeterProvider(meterProvider);
};
