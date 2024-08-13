import 'nhsuk-frontend/dist/nhsuk.css';
import './global.css';
import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { NhsHeader } from '@components/nhs-header';
import { fetchUserProfile } from '@services/appointmentsService';
import { When } from '@components/when';
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
        <main>
          <NhsHeader userEmail={userProfile?.emailAddress} />
          <When condition={userProfile === undefined}>
            <div className="nhsuk-grid-row nhsuk-main-wrapper nhsuk-width-container">
              <div className="nhsuk-grid-column-full">
                <h1>Appointment Management Service</h1>
                <NhsWarning title="You cannot access this site">
                  <p>
                    You are currently not signed in. To use this site, please
                    sign in.
                  </p>
                </NhsWarning>
              </div>
            </div>
          </When>
          <When condition={userProfile !== undefined}>{children}</When>
        </main>
      </body>
    </html>
  );
}
