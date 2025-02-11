import NhsAnonymousPage from '@components/nhs-anonymous-page';
import { getCookieConsent } from '@services/cookiesService';
import CookieConsentForm from './cookie-consent-form';
import { Table } from '@components/nhsuk-frontend';
import Details from '@components/nhsuk-frontend/details';

const Page = async () => {
  const consentCookie = await getCookieConsent();

  return (
    <NhsAnonymousPage title="Cookie policy" originPage="cookie-policy">
      <section>
        <h3>What are cookies?</h3>
        <p>
          Cookies are files saved on your phone, tablet or computer when you
          visit a website or service.
        </p>
        <p>
          They store information about how you use the website or service, such
          as the pages you visit.
        </p>
        <p>
          Cookies are not viruses or computer programs. They are very small so
          do not take up much space.
        </p>
      </section>

      <section>
        <h3>How we use cookies</h3>
        <p>We only use cookies to:</p>
        <ul>
          <li>make our service work</li>
          <li>
            measure how you use our website, such as which links you click on
            (analytics cookies), if you give us permission
          </li>
        </ul>
        <p>
          We do not use any other cookies, for example, cookies that remember
          your settings or cookies that help with health campaigns.
        </p>
      </section>

      <section>
        <h3>Cookies that make our service work</h3>
        <p>We use cookies to keep our service secure and fast.</p>
        <Details summary="List of cookies that make our website work">
          <Table
            headers={['Cookie name', 'Purpose', 'Expiry']}
            rows={[
              [
                'token',
                'Tracks your authorisation during a session',
                'When you close the browser',
              ],
              [
                'ams-notification',
                'Provides you with information when actions are taken (for example, a notification)',
                'After 15 seconds',
              ],
              [
                'previousPage',
                'Allows you to return to the page you were on after you have been logged out and back in again',
                'When you close the browser',
              ],
            ]}
          />
        </Details>
      </section>

      <section>
        <h3>Cookies that measure website use</h3>
        <p>
          We also like to use analytics cookies. These cookies store anonymous
          information about how you use our website, such as which pages you
          visit or what you click on.
        </p>
        <Details summary="List of cookies that measure website use">
          <Table
            headers={['Cookie name', 'Purpose', 'Expiry']}
            rows={[
              [
                'AMCV_#',
                `Used by Adobe Analytics. Tells us if you've used our site before.`,
                '2 years',
              ],
              [
                'AMCVS_#AdobeOrg',
                'Used by Adobe Analytics. Tells us how you use our site.',
                'When you close the browser',
              ],
              [
                'com.adobe.reactor.dataElementCookiesMigrated',
                'Used by Adobe Analytics. Includes data elements set to capture usage of our website.',
                'When you close the browser',
              ],
              [
                'demdex',
                'Used by Adobe Analytics. Allows cross-domain analytics of NHS properties.',
                '180 days',
              ],
              [
                's_cc',
                'Used by Adobe Analytics. Checks if cookies work in your web browser.	',
                'When you close the browser',
              ],
              [
                's_getNewRepeat',
                `Used by Adobe Analytics. Tells us if you've used our website before.`,
                '30 days',
              ],
              [
                's_ppn',
                'Used by Adobe Analytics. Tells us how you use our website by reading the previous page you visited.',
                '1 day',
              ],
              [
                's_sq',
                'Used by Adobe Analytics. Remembers the last link you clicked on.',
                'When you close the browser',
              ],
            ]}
          />
        </Details>
      </section>

      <CookieConsentForm consentCookie={consentCookie} />
    </NhsAnonymousPage>
  );
};

export default Page;
