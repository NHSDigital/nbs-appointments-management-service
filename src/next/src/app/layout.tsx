import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { fetchSites } from "./lib/sitesService";
import { When } from "./components/when";
import { AuthWrapper } from "./components/authwrapper";
import Providers from "./context/providers";
import ActionSuccess from "./components/action-success";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Appointment Service",
  description: "Next Appointment Service",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {

  const sites = await fetchSites();

  return (
    <html lang="en">
      <body className={inter.className}>
        <div style={{background: "#326499", padding: "10px", borderBottom: "solid 1px gray", color: "White"}}>
          NHS Appointments Book
        </div>
        <Providers>
          <ActionSuccess />
          <When condition={sites.length === 0}>
            <AuthWrapper />
          </When>
          <When condition={sites.length > 0}>
            {children}
          </When>
        </Providers>
      </body>
    </html>
  );
}
