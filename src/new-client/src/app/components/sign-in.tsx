'use client'
import { useSearchParams, usePathname,  } from "next/navigation";
import React, { useTransition } from "react";
import { fetchAccessToken } from "../lib/auth";
import { When } from "./when";

export const SignIn = ({authenticateUrl}: {authenticateUrl: string}) => {
    const searchParams = useSearchParams();
    const hasCode = searchParams.has("code");
    const code = searchParams.get("code") ?? "";
    const pathname = usePathname();
    const [, startTransition] = useTransition();

    React.useEffect(() => {
        if(hasCode) {
            
            startTransition(async () => {
                await fetchAccessToken(code)
            })
        }
    },[hasCode, pathname, code, searchParams])

    return (
            <When condition={!hasCode}>
                <div style={{margin: "20px"}}>
                <a className="nhsuk-button"
                    href={`${authenticateUrl}?redirect_uri=${window.location.href}`}>Sign In</a>
                </div>
            </When>
    )
}