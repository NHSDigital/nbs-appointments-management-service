class FeatureFlagClient {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  public async overrideFeatureFlag(featureFlag: string, enabled: boolean) {
    return fetch(
      `${this.baseUrl}/api/feature-flag-override/${featureFlag}?enabled=${enabled}`,
      {
        method: 'PATCH',
      },
    ).then(async response => {
      if (!response.ok) {
        throw new Error(
          `Failed to override feature flag ${featureFlag} to ${enabled}: ${response.status} ${response.statusText}`,
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
