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
  Expander,
} from '@nhsuk-frontend-components';
import { useRouter } from 'next/navigation';
import { SubmitHandler, useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { SetSiteDetailsRequest, Site } from '@types';
import { saveSiteDetails } from '@services/appointmentsService';
import DecimalFormControl from '@components/form-controls/decimal';
import PhoneNumberFormControl from '@components/form-controls/phoneNumber';
import {
  editSiteDetailsFormSchema,
  EditSiteDetailsFormValues,
} from './edit-site-details-form-schema';

const EditDetailsForm = ({ site }: { site: Site }) => {
  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting, isSubmitSuccessful },
  } = useForm<EditSiteDetailsFormValues>({
    defaultValues: {
      name: site.name,
      //add in line breaks at each comma
      address: site.address.replace(/, /g, ',\n'),
      phoneNumber: site.phoneNumber,
      latitude: site.location.coordinates[1],
      longitude: site.location.coordinates[0],
    },
    resolver: yupResolver(editSiteDetailsFormSchema),
  });

  const { replace } = useRouter();

  const submitForm: SubmitHandler<EditSiteDetailsFormValues> = async (
    form: EditSiteDetailsFormValues,
  ) => {
    const payload: SetSiteDetailsRequest = {
      name: form.name.trim(),
      //remove the line breaks and save back
      address: form.address.replace(/\n/g, ' ').trim(),
      phoneNumber: form.phoneNumber?.trim(),
      latitude: `${form.latitude}`,
      longitude: `${form.longitude}`,
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

      <Expander summary={'View the latitude and longitude ranges'}>
        <ul>
          <li>the minimum latitude is 49.8 and the maximum latitude is 60.9</li>
          <li>
            the minimum longitude is -8.1 and the maximum longitude is 1.8
          </li>
        </ul>
      </Expander>
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
