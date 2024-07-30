'use client'

type ErrorProps = {
    error: Error & { digest?: string };
    reset: () => void;
  }

const ErrorPage = ({error, reset} : ErrorProps) => {
    return (
        <main className="flex h-full flex-col items-center justify-center">
            <h2 className="text-center">Something went wrong!</h2>
            <h2 className="text-center">Maybe you don't have permission to view the page!</h2>
            <div>
                {JSON.stringify(error)}
            </div>
            <button
                className="mt-4 rounded-md bg-blue-500 px-4 py-2 text-sm text-white transition-colors hover:bg-blue-400"
                onClick={
                // Attempt to recover by trying to re-render the invoices route
                () => reset()
                }
            >
                Try again
            </button>
    </main>
    )
}

export default ErrorPage