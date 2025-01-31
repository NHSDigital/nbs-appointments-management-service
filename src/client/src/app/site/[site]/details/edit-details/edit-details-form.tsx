/* eslint-disable react/jsx-props-no-spreading */
'use client';
import React from 'react';
import {
  Button,
  FormGroup,
  ButtonGroup,
  SmallSpinnerWithText,
  TextInput,
  TextArea,
} from '@nhsuk-frontend-components';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { SetSiteDetailsRequest, SiteWithAttributes } from '@types';
import { saveSiteDetails } from '@services/appointmentsService';
import { DECIMAL_REGEX, PHONE_NUMBER_REGEX } from '../../../../../constants';

type FormFields = {
  name: string;
  address: string;
  phoneNumber: string;
  latitude: string;
  longitude: string;
};

const EditDetailsForm = ({
  siteWithAttributes,
}: {
  siteWithAttributes: SiteWithAttributes;
}) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: {
      name: siteWithAttributes.name,
      //add in line breaks at each comma
      address: siteWithAttributes.address.replace(/, /g, ',\n'),
      phoneNumber: siteWithAttributes.phoneNumber,
      latitude: siteWithAttributes.location.coordinates[0].toString(),
      longitude: siteWithAttributes.location.coordinates[1].toString(),
    },
  });

  const { replace } = useRouter();

  const submitForm: SubmitHandler<FormFields> = async (form: FormFields) => {
    const payload: SetSiteDetailsRequest = {
      name: form.name.trim(),
      //remove the line breaks and save back
      address: form.address.replace(/\n/g, ' ').trim(),
      phoneNumber: form.phoneNumber.trim(),
      latitude: form.latitude.trim(),
      longitude: form.longitude.trim(),
    };
    await saveSiteDetails(siteWithAttributes.id, payload);

    replace(`/site/${siteWithAttributes.id}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup
        error={errors.name ? 'You have not entered a name' : undefined}
      >
        <TextInput
          id="name"
          label="Site name"
          {...register('name', {
            required: true,
          })}
        ></TextInput>
      </FormGroup>

      <FormGroup
        error={errors.address ? 'You have not entered an address' : undefined}
      >
        <TextArea
          id="address"
          label="Site address"
          {...register('address', {
            required: true,
          })}
        ></TextArea>
      </FormGroup>

      <FormGroup
        error={
          errors.latitude || errors.longitude
            ? 'You have not entered valid coordinates'
            : undefined
        }
      >
        <TextInput
          id="latitude"
          label="Latitude"
          {...register('latitude', {
            required: true,
            pattern: DECIMAL_REGEX,
          })}
        ></TextInput>
        <TextInput
          id="longitude"
          label="Longitude"
          {...register('longitude', {
            required: true,
            pattern: DECIMAL_REGEX,
          })}
        ></TextInput>
      </FormGroup>

      <FormGroup
        error={
          errors.phoneNumber
            ? 'You have not entered a valid phone number'
            : undefined
        }
      >
        <TextInput
          id="phoneNumber"
          type="tel"
          label="Phone number"
          {...register('phoneNumber', {
            required: true,
            pattern: PHONE_NUMBER_REGEX,
          })}
        ></TextInput>
      </FormGroup>

      {isSubmitting || isSubmitSuccessful ? (
        <SmallSpinnerWithText text="Updating details..." />
      ) : (
        <ButtonGroup>
          <Button type="submit">Save and continue</Button>
        </ButtonGroup>
      )}
    </form>
  );
};

export default EditDetailsForm;
