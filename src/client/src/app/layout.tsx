
import '../index.css'
import type { Metadata } from 'next'

export const metadata: Metadata = {
    title: "NHS Appointments Book",
    description: "NHS Appointments Book"
}

export default function RootLayout({
    children,
  }: {
    children: React.ReactNode
  }) {
    return (
        <html lang="en">
            <body>
                <noscript>You need to enable JavaScript to run this app.</noscript>
                <div id="root">{children}</div>
            </body>
        </html>
    )
  }