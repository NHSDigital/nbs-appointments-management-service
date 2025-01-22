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
      //strip out whitespace from phone number so that it can be a 'number' control
      phoneNumber: siteWithAttributes.phoneNumber.replace(/\s/g, ''),
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
      <h3>Site name</h3>
      <FormGroup error={errors.name?.message}>
        <TextInput id="name" {...register('name')}></TextInput>
      </FormGroup>

      <FormGroup error={errors.address?.message}>
        <h3>Site address</h3>
        <TextArea id="address" label="" {...register('address')}></TextArea>
      </FormGroup>

      <h3>Latitude and longitude</h3>
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

      <h3>Phone number</h3>
      <FormGroup error={errors.phoneNumber?.message}>
        <TextInput
          id="phoneNumber"
          type="tel"
          pattern="\d*"
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
