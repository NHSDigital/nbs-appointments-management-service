import { LATEST_CONSENT_COOKIE_VERSION } from '@constants';
import { getCookieConsent } from '@services/cookiesService';
import { cookies } from 'next/headers';

const CookieBanner = async () => {
  const consentCookie = await getCookieConsent();
  const cookieStore = cookies();
  const recentlyUpdatedConsent = cookieStore.get(
    'nhsuk-mya-cookie-consent-updated',
  );

  if (
    consentCookie === undefined ||
    consentCookie.version !== LATEST_CONSENT_COOKIE_VERSION
  ) {
    return (
      <div>
        <div id="nhsuk-cookie-banner">
          <div className="nhsuk-cookie-banner" id="cookiebanner">
            <div className="nhsuk-width-container">
              <h2>Cookies on the NHS website</h2>
              <p>
                We've put some small files called cookies on your device to make
                our site work.
              </p>
              <p>
                We'd also like to use analytics cookies. These collect feedback
                and send information about how our site is used to services
                called Adobe Analytics, Adobe Target, Qualtrics Feedback and
                Google Analytics. We use this information to improve our site.
              </p>
              <p>
                Let us know if this is OK. We'll use a cookie to save your
                choice. You can{' '}
                <a
                  id="nhsuk-cookie-banner__link"
                  href="/our-policies/cookies-policy/"
                >
                  read more about our cookies
                </a>{' '}
                before you choose.
              </p>
              <ul>
                <li>
                  <button
                    className="nhsuk-button"
                    id="nhsuk-cookie-banner__link_accept_analytics"
                  >
                    I'm OK with analytics cookies
                  </button>
                </li>
                <li>
                  <button
                    className="nhsuk-button"
                    id="nhsuk-cookie-banner__link_accept"
                  >
                    Do not use analytics cookies
                  </button>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (recentlyUpdatedConsent) {
    return (
      <div
        className="nhsuk-success-banner"
        id="nhsuk-cookie-confirmation-banner"
      >
        <div className="nhsuk-width-container">
          <p id="nhsuk-success-banner__message">
            You can change your cookie settings at any time using our{' '}
            <a href="/our-policies/cookies-policy/">cookies page</a>.
          </p>
        </div>
      </div>
    );
  }

  return null;
};

export default CookieBanner;
