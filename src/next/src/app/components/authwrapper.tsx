'use client'
import { useSearchParams, usePathname } from "next/navigation";
import React, { useTransition } from "react";
import { fetchAccessToken } from "../lib/usersService";
import { When } from "./when";

export const AuthWrapper = () => {
    const searchParams = useSearchParams();
    const hasCode = searchParams.has("code");
    
    const params = new URLSearchParams(searchParams);    
    const pathname = usePathname();
    const [returnUrl, setReturnUrl] = React.useState<string>("");
    const [pending, startTransition] = useTransition();

    React.useEffect(() => {        
        const path = pathname.length == 1 ? "" : pathname
        const mark = params.size > 0 ? "?" : ""
        setReturnUrl(`http://localhost:3000${path}${mark}${params.toString()}`)
        if(hasCode) {
            const code = searchParams.get("code");
            startTransition(async () => {
                await fetchAccessToken(code!)
            })
        }
    },[])

    return (
        <>
            <When condition={!hasCode}>
                <div style={{margin: "20px"}}>
                <a
                    className="ml-3 px-3 py-2 text-xs font-medium text-center text-white bg-blue-700 rounded-lg hover:bg-blue-800 focus:ring-4 focus:outline-none focus:ring-blue-300 dark:bg-blue-600 dark:hover:bg-blue-700 dark:focus:ring-blue-800" 
                    href={`http://localhost:7071/api/authenticate?redirect_uri=${returnUrl}`}>Sign In</a>
                </div>
            </When>
        </>
    )
}

