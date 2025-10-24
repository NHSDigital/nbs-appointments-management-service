import { FeatureFlag } from '@e2etests/types';

class FeatureFlagClient {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  public async overrideFeatureFlag(featureFlag: FeatureFlag) {
    return fetch(
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
  }

  public async clearAllFeatureFlagOverrides() {
    return fetch(`${this.baseUrl}/api/feature-flag-overrides-clear`, {
      method: 'PATCH',
    }).then(async response => {
      if (!response.ok) {
        throw new Error(
          `Failed to clear feature flag overrides: ${response.status} ${response.statusText}`,
        );
      }
    });
  }
}

export default FeatureFlagClient;
