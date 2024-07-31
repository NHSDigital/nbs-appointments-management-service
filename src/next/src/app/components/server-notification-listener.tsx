"use client"
import React from "react";
import { useCookies } from "next-client-cookies";
import { useNotifications } from "../context/notifications";

const ServerNotificationListener = () => {
    const cookies = useCookies();
    const { addNotification } = useNotifications();

    React.useEffect(() => {
        const notificationCookie = cookies.get("notification");
        if(notificationCookie) {
            addNotification(notificationCookie);
            cookies.remove("notification")
        }
    },[])

    return null;
}

export default ServerNotificationListener