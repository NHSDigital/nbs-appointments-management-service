import {useAuthenticatedClient} from "./ApiClient";

export const useUserService = () => {
    const client = useAuthenticatedClient();
    
    const getUserPermissions = async (site: string): Promise<any> => {
        return client.get(`user/permissions?site=${site}`)
            .then(rsp => {
                return rsp.json();
            })
    }
    return {getUserPermissions}
}