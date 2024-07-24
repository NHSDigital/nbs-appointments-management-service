import { Site } from "../Types/Site";
import { SiteConfiguration } from "../Types/SiteConfiguration";
import { useAuthenticatedClient } from "./ApiClient";

export const useSiteConfigurationService = () => {

      const client = useAuthenticatedClient();

      const setSiteConfiguration = async (siteConfiguration: SiteConfiguration) => {
            return await client.post(`site-configuration`, siteConfiguration);
      }

      const getSiteConfiguration = async (siteId: string): Promise<SiteConfiguration | null> => {
            var response = await client.get(`site-configuration?site=${siteId}`)
            if (response.status === 404) {
                  return null;
            }
            return await response.json() as SiteConfiguration;
      }

      const getSitesForUser = async (): Promise<Site[]> => {
            var response = await client.get('user/sites')
            return await response.json() as Site[];
      }

      return { getSiteConfiguration, getSitesForUser, setSiteConfiguration }
}

