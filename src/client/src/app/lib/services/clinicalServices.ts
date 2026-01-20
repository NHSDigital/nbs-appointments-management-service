import { ClinicalService } from '@types';

export const SERVICE_TYPE_TITLES: Record<string, string> = {
  flu: 'Flu services',
  'COVID-19': 'COVID-19 services',
  'COVID-19 and flu': 'Flu and COVID-19 co-admin services',
  RSV: 'RSV services',
  'RSV and COVID-19': 'RSV and COVID-19 co-admin services',
};

export const groupServicesByType = (services: ClinicalService[]) => {
  return services.reduce(
    (acc, service) => {
      const group = service.serviceType;
      if (!acc[group]) {
        acc[group] = [];
      }
      acc[group].push(service);
      return acc;
    },
    {} as Record<string, ClinicalService[]>,
  );
};
