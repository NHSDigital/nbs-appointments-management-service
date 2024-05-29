import { useAuthContext } from "../ContextProviders/AuthContextProvider";
import { getApiUrl } from "../configuration"

export const useAuthenticatedClient = () => {
    const { idToken, signOut } = useAuthContext();

    const post = async (path: string, body: any) : Promise<Response> => {
        const response = await fetch(getApiUrl(path), 
          { method: "POST", 
            headers: {
            Authorization: `Bearer ${idToken}`},
            body: JSON.stringify(body)});
          if(response.status === 401) {
              signOut();
          }
          return response;
    }

    const get = async (path: string) : Promise<Response> => {
      const response = await fetch(getApiUrl(path), 
        { method: "GET", 
          headers: { Authorization: `Bearer ${idToken}`}});
      if(response.status === 401) {
            signOut();
        }
        return response;
    }

    return {post, get}
}
