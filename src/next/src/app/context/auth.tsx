'use client'
import React from "react";

export interface IAuthContext {
    hasPermission: (permission: string) => boolean
}

export const AuthContext = React.createContext<IAuthContext | null>(null);

type Props = { 
    permissions: string[]
    children: React.ReactNode 
}

export const AuthContextProvider = ({permissions, children} : Props) => {

    const hasPermission = (permission: string) => permissions.includes(permission)

    return (
        <AuthContext.Provider value={{hasPermission}}>
                {children}
        </AuthContext.Provider>
    );
}

export const useAuthContext = () => {
    const context = React.useContext(AuthContext);
    if (context) return context;

    throw Error("Auth context was not registered");
};
