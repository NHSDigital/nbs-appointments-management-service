import {useAuthenticatedClient} from "./ApiClient";

export const useUserService = () => {
    const client = useAuthenticatedClient();
    
    const getUserPermissions = async (site: string): Promise<any> => {
        return client.get(`user/permissions?site=${site}`)
            .then(rsp => {
                return rsp.json();
            })
    }
    
    const setUserRoles = async (site: string, user: string, roles: string[]) => {
        const payload = {
            scope: `site:${site}`,
            user,
            roles
        }
        return client.post('user/roles', payload)
    }
    return {getUserPermissions, setUserRoles}
}