import 'nhsuk-frontend/dist/nhsuk.css';
import './styles/global.css';
import './styles/nhsuk-frontend-settings.scss';
import './styles/cookie-banner.css';
import { Inter } from 'next/font/google';
import CookieBanner from '@components/cookie-banner';

const inter = Inter({ subsets: ['latin'] });

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <CookieBanner />
        {children}
      </body>
    </html>
  );
}
