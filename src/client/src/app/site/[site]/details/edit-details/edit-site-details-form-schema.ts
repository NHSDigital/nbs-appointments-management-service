import * as yup from 'yup';

export type EditSiteDetailsFormValues = {
  name: string;
  address: string;
  phoneNumber: string;
  latitude: number;
  longitude: number;
};

const PHONE_NUMBER_REGEX = new RegExp(/^[0-9 ]*$/);

export const editSiteDetailsFormSchema: yup.ObjectSchema<EditSiteDetailsFormValues> =
  yup
    .object({
      name: yup.string().trim().required('Enter a name'),
      address: yup.string().trim().required('Enter an address'),
      phoneNumber: yup
        .string()
        .trim()
        .required('Enter a phone number')
        .test('is-valid-phone-number', 'Enter a valid phone number', value => {
          return value ? PHONE_NUMBER_REGEX.test(value) : true;
        }),
      latitude: yup
        .number()
        .transform(value => (isNaN(value) ? undefined : value))
        .required('Enter a latitude')
        .min(-49.8, 'Enter a valid latitude')
        .max(60.9, 'Enter a valid latitude'),
      longitude: yup
        .number()
        .transform(value => (isNaN(value) ? undefined : value))
        .required('Enter a longitude')
        .min(-8.1, 'Enter a valid longitude')
        .max(1.8, 'Enter a valid longitude'),
    })
    .required();
