/* eslint-disable no-console */
import { FeatureFlag } from '@e2etests/types';

class FeatureFlagClient {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  public async overrideFeatureFlag(featureFlag: FeatureFlag) {
    await fetch(
      `${this.baseUrl}/api/feature-flag-override/${featureFlag.name}?enabled=${featureFlag.enabled}`,
      {
        method: 'PATCH',
      },
    ).then(async response => {
      if (!response.ok) {
        throw new Error(
          `Failed to override feature flag ${featureFlag.name} to ${featureFlag.enabled}: ${response.status} ${response.statusText}`,
        );
      }
    });

    console.log(
      `Toggled: ${featureFlag.name} to ${featureFlag.enabled ? 'ON' : 'OFF'}.`,
    );
    return;
  }
}

export default FeatureFlagClient;
