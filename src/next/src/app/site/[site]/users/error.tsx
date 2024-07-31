'use client'

import { useRouter } from "next/navigation";

const ErrorPage = () => {
    const router = useRouter();

    return (
        <main className="flex h-full flex-col items-center justify-center">
            <h2 className="text-center">Something went wrong!</h2>
            <button
                className="mt-4 rounded-md bg-blue-500 px-4 py-2 text-sm text-white transition-colors hover:bg-blue-400"
                onClick={
                () => router.refresh()
                }
            >
                Try again
            </button>
    </main>
    )
}

export default ErrorPage