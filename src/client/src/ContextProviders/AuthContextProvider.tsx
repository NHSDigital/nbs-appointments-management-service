import React from "react";
import { When } from "../Components/When";
import { SignIn } from "../Components/SignIn";
import { useSearchParams } from "react-router-dom";
import { getApiUrl } from "../configuration";
import { jwtDecode } from "jwt-decode";

export interface IAuthContext {
    idToken: string | null;
    getUserEmail: () => string;
    signOut: () => void;
}

export const AuthContext = React.createContext<IAuthContext | null>(null);

export const AuthContextProvider = ({ children }: { children: React.ReactNode }) => {
    const [searchParams] = useSearchParams();
    const idToken = localStorage.getItem("idtoken");
    const host = `${window.location.protocol}//${window.location.host}`

    const getUserEmail = () => {
        const decoded = jwtDecode(idToken!) as any;
        
        if(decoded["Email Address"])
            return decoded["Email Address"];
        if(decoded["email"])
            return decoded["email"];
        if(decoded["sub"])
            return decoded["sub"];

        return ""
    }
    
    const signIn = () => {
        window.location.replace(`${getApiUrl("authenticate")}?redirect_uri=${host}`);
      }

    const signOut = () => {
        localStorage.removeItem("idtoken");
        window.location.replace(host);
    }

    if (searchParams.has("code")) {
        const code = searchParams.get("code");
        fetch(getApiUrl("token"), { method: "POST", body: code })
            .then(rsp => {
                if (rsp.ok) return rsp.json()
            }).then(authObj => {
                if (authObj.token) {
                    localStorage.setItem("idtoken", authObj.token)
                    window.location.replace(host)
                }
            });
    }

    return (
        <AuthContext.Provider value={{idToken, signOut, getUserEmail}}>
            <When condition={idToken !== null}>
                {children}
            </When>
            <When condition={idToken === null}>
                <SignIn signIn={signIn} />
            </When>
        </AuthContext.Provider>
    );
}

export const useAuthContext = () => {
    const context = React.useContext(AuthContext);
    if (context) return context;

    throw Error("Auth context was not registered");
};
