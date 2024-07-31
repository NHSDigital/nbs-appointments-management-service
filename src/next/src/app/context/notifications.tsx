"use client"
import React, { ReactNode } from "react";


export interface INotificationsContext {
    messages: string[]
    addNotification: (notification: string) => void;
}

const messageVals: { [id: string] : string; }  = {
    "user_saved": "User roles assigned successfully.",
    "no_permission": "The action could not be completed. You do not have sufficient permissions",
    "no_auth": "You are not signed in",
    "action_failed": "The action could not be completed."
}

export const NotificationsContext = React.createContext<INotificationsContext | null>(null);

export const NotificationsContextProvider = ({children} : {children: ReactNode}) => {
    const [messages, setMessages] = React.useState([] as string[]);

    const addNotification = (key: string) => {
        const message = messageVals[key];
        setMessages([...messages, message]);
    }
    
    return (
        <NotificationsContext.Provider value={{messages, addNotification}}>
                {children}
        </NotificationsContext.Provider>
    );
}

export const useNotifications = () => {
    const context = React.useContext(NotificationsContext);
    if (context) return context;

    throw Error("Notification context was not registered");
};