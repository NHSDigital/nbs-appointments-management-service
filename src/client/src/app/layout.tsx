import './styles/custom-nhsuk.scss';
import './styles/global.css';
import './styles/cookie-banner.css';
import './styles/print.css';
import { Inter } from 'next/font/google';
import CookieBanner from '@components/cookie-banner';
import { AppInsightsInitializer } from './appInsightsInitializer';

const inter = Inter({ subsets: ['latin'] });

const connectionString = process.env.APP_INSIGHTS_CONNECTION_STRING;

if (!connectionString) {
  throw new Error(
    'APP_INSIGHTS_CONNECTION_STRING is not defined. Please set it in your environment variables.',
  );
}

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <AppInsightsInitializer connectionString={connectionString as string} />
        <CookieBanner />
        {children}
      </body>
    </html>
  );
}
