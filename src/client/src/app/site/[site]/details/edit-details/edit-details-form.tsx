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
      name: form.name,
      //remove the line breaks and save back
      address: form.address.replace(/\n/g, ' '),
      phoneNumber: form.phoneNumber,
      latitude: form.latitude,
      longitude: form.longitude,
    };
    await saveSiteDetails(siteWithAttributes.id, payload);

    replace(`/site/${siteWithAttributes.id}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup error={errors.name?.message}>
        <TextInput
          id="name"
          label="Site name"
          {...register('name')}
        ></TextInput>
      </FormGroup>

      <FormGroup error={errors.address?.message}>
        <TextArea
          id="address"
          label="Site address"
          {...register('address')}
        ></TextArea>
      </FormGroup>

      <FormGroup error={errors.latitude?.message || errors.longitude?.message}>
        <TextInput
          id="latitude"
          label="Latitude"
          {...register('latitude')}
        ></TextInput>
        <TextInput
          id="longitude"
          label="Longitude"
          {...register('longitude')}
        ></TextInput>
      </FormGroup>

      <FormGroup error={errors.phoneNumber?.message}>
        <TextInput
          id="phoneNumber"
          type="tel"
          label="Phone number"
          title="Please enter numbers and spaces only."
          pattern="[0-9 ]*"
          {...register('phoneNumber')}
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
