import { load } from '@azure/app-configuration-provider';
import {
  FeatureManager,
  ConfigurationMapFeatureFlagProvider,
} from '@microsoft/feature-management';

const connectionString = process.env.AZURE_APPCONFIG_CONNECTION_STRING ?? '';

const tenMinutes = 1000 * 60 * 10;

const isEnabled = async (feature: string) => {
  const settings = await load(connectionString, {
    featureFlagOptions: {
      enabled: true,
      selectors: [
        {
          keyFilter: '*',
        },
      ],
      refresh: {
        enabled: true,
        refreshIntervalInMs: tenMinutes,
      },
    },
  });

  const featureFlagProvider = new ConfigurationMapFeatureFlagProvider(settings);
  const featureManager = new FeatureManager(featureFlagProvider);

  return await featureManager.isEnabled(feature);
};

export default isEnabled;
