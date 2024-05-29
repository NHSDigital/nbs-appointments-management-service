import { SiteConfiguration, SiteInformation } from "../Types/SiteConfiguration";
import { useAuthenticatedClient } from "./ApiClient";

export const useSiteConfigurationService = () => {

      const client = useAuthenticatedClient();
      
       const setSiteConfiguration = async (siteConfiguration: SiteConfiguration) : Promise<void> => {
            return client.post(`site-configuration`, siteConfiguration)
                  .then(rsp => {
                        if (!rsp.ok) throw new Error("An error occurred setting site configuration");
                        return;
            });
        }

        const getSiteConfigurationForUser = async () : Promise<SiteInformation|null> => {
            var response = await client.get(`site-configuration?user`)
            if(response.status === 404)
                  return null;
            if(!response.ok) {
                  throw new Error("An error occurred getting site configuration");
            }
            return await response.json() as SiteInformation;
        }

        return {getSiteConfigurationForUser, setSiteConfiguration}
}