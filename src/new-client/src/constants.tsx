export const EMAIL_REGEX = new RegExp(/[\w-.]+@nhs.net/i);
export const URL_REGEX = new RegExp(
  '([a-zA-Z0-9]+://)?([a-zA-Z0-9_]+:[a-zA-Z0-9_]+@)?([a-zA-Z0-9.-]+\\.[A-Za-z]{2,4})(:[0-9]+)?([^ ])+',
);
export const SPECIAL_CHARACTER_REGEX = /^[-\w \.\,\-]+$/;
