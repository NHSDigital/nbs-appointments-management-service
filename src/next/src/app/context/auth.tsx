'use client'
import React from "react";

export interface IAuthContext {
    signIn: () => void;
    getUserEmail: () => string;
}

export const AuthContext = React.createContext<IAuthContext | null>(null);

export const AuthContextProvider = ({ children }: { children: React.ReactNode }) => {
    const [ email, setEmail ] = React.useState<string>("test@nhs.net");

    const getUserEmail = () => email!;
    
    const signIn = () => {
        setEmail("cc.agent@nhs.net")
    }

    return (
        <AuthContext.Provider value={{signIn, getUserEmail}}>
            {children}
        </AuthContext.Provider>
    );
}

export const useAuthContext = () => {
    const context = React.useContext(AuthContext);
    if (context) return context;

    throw Error("Auth context was not registered");
};