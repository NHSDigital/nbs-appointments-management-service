'use client'

import { ReactNode } from "react"
import { AuthContextProvider } from "./auth"

const Providers = ({children} : {children: ReactNode}) => (
    <AuthContextProvider>
        {children}
    </AuthContextProvider>
)

export default Providers