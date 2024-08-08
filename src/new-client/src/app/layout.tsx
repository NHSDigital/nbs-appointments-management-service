import 'nhsuk-frontend/dist/nhsuk.css';
import './global.css';
import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { NhsHeader } from '@components/nhs-header';
import { When } from '@components/when';
import { SignIn } from '@components/sign-in';
import { fetchUserProfile } from '@services/nbsService';

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
  const userProfile = await fetchUserProfile();

  return (
    <html lang="en">
      <body className={inter.className}>
        <main>
          <NhsHeader userEmail={userProfile?.emailAddress} />
          <When condition={userProfile === undefined}>
            <SignIn />
          </When>
          <When condition={userProfile !== undefined}>{children}</When>
        </main>
      </body>
    </html>
  );
}
