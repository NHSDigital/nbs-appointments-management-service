import 'nhsuk-frontend/dist/nhsuk.css';
import './global.css';
import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { NhsHeader } from '@components/nhs-header';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'Appointment Management Service',
  description: 'A National Booking Service site for managing NHS appointments',
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <main>
          <NhsHeader />
          {children}
        </main>
      </body>
    </html>
  );
}
