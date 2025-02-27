import { registerOTel } from '@vercel/otel';

export function register() {
  console.log('Registering OpenTelemetry instrumentation...');
  registerOTel({ serviceName: 'mya-web-app' });
}
