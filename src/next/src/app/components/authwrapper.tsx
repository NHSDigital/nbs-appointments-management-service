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
                <a href={`http://localhost:7071/api/authenticate?redirect_uri=${returnUrl}`}>Sign In</a>
            </When>
            <When condition={pending}>
                Authenticating...
            </When>
        </>
    )
}

