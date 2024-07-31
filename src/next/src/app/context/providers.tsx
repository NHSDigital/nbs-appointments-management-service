'use client'

import { ReactNode } from "react"
import { NotificationsContextProvider } from "./notifications"

const Providers = ({children} : {children: ReactNode}) => (
    <NotificationsContextProvider>
        {children}
    </NotificationsContextProvider>
)

export default Providers