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
import { SetSiteDetailsRequest, Site } from '@types';
import { saveSiteDetails } from '@services/appointmentsService';
import DecimalFormControl from '@components/form-controls/decimal';
import PhoneNumberFormControl from '@components/form-controls/phoneNumber';

type FormFields = {
  name: string;
  address: string;
  phoneNumber: string;
  latitude: string;
  longitude: string;
};

const EditDetailsForm = ({ site }: { site: Site }) => {
  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<FormFields>({
    defaultValues: {
      name: site.name,
      //add in line breaks at each comma
      address: site.address.replace(/, /g, ',\n'),
      phoneNumber: site.phoneNumber,
      latitude: site.location.coordinates[1].toString(),
      longitude: site.location.coordinates[0].toString(),
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
    await saveSiteDetails(site.id, payload);

    replace(`/site/${site.id}/details`);
  };

  return (
    <form onSubmit={handleSubmit(submitForm)}>
      <FormGroup error={errors.name?.message}>
        <TextInput
          id="name"
          label="Site name"
          {...register('name', {
            required: {
              value: true,
              message: 'Enter a name',
            },
          })}
        ></TextInput>
      </FormGroup>

      <FormGroup error={errors.address?.message}>
        <TextArea
          id="address"
          label="Site address"
          {...register('address', {
            required: {
              value: true,
              message: 'Enter an address',
            },
          })}
        ></TextArea>
      </FormGroup>

      <DecimalFormControl
        formField="latitude"
        label="Latitude"
        control={control}
        errors={errors}
      />

      <DecimalFormControl
        formField="longitude"
        label="Longitude"
        control={control}
        errors={errors}
      />

      <PhoneNumberFormControl
        formField="phoneNumber"
        label="Phone number"
        control={control}
        errors={errors}
      />

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
