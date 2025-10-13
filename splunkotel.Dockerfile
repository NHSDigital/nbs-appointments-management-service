FROM quay.io/signalfx/splunk-otel-collector:latest

COPY ./splunk-otel-collector-config.yaml ./etc/splunk-otel-collector-config.yaml

CMD ["--config=/etc/splunk-otel-collector-config.yaml"]