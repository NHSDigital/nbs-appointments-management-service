export const URL_REGEX = new RegExp(
  '([a-zA-Z0-9]+://)?([a-zA-Z0-9_]+:[a-zA-Z0-9_]+@)?([a-zA-Z0-9.-]+\\.[A-Za-z]{2,4})(:[0-9]+)?([^ ])+',
);
export const SPECIAL_CHARACTER_REGEX = /^[ A-Za-z0-9.,-]*$/;

/**
 * The current version of the NHS MYA consent cookie, implicitly paired with the text on the cookie acceptance page.
 * If the cookie acceptance text changes, this version should be incremented to prompt users to re-accept.
 */
export const LATEST_CONSENT_COOKIE_VERSION = 1;
