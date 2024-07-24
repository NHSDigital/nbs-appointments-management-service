import {useAuthenticatedClient} from "./ApiClient";

export const useRolesService = () => {
    const client = useAuthenticatedClient();
    
    const getRoles = async (): Promise<any> => {
        return client.get('roles')
            .then(rsp => rsp.json()
                .then(j => j.roles))
    }
    
    return { getRoles }
}