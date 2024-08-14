import 'nhsuk-frontend/dist/nhsuk.css';
import './global.css';
import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { NhsHeader } from '@components/nhs-header';
import { fetchUserProfile } from '@services/appointmentsService';
import { When } from '@components/when';
import NhsPage from '@components/nhs-page';
import NhsWarning from '@components/nhs-warning';

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
        <a className="nhsuk-skip-link" href="#main-content">
          Skip to main content
        </a>
        <NhsHeader userEmail={userProfile?.emailAddress} />
        <When condition={userProfile === undefined}>
          <NhsPage title="Appointment Management Service">
            <NhsWarning title="You cannot access this site">
              <p>
                You are currently not signed in. To use this site, please sign
                in.
              </p>
            </NhsWarning>
          </NhsPage>
        </When>
        <When condition={userProfile !== undefined}>{children}</When>
      </body>
    </html>
  );
}
