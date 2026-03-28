import { useState, useEffect } from 'react';

import { useServicesStore } from '@/features/services/stores/servicesStore';
import { useCategoriesStore } from '@/features/services/stores/categoriesStore';
import { useGarmentTypesStore } from '@/features/services/stores/garmentTypesStore';
import { useServiceGarmentsStore } from '@/features/services/stores/serviceGarmentsStore';
import { useDiscountsStore } from '@/features/services/stores/discountsStore';

import { ServicesRail } from '../components/ServicesRail';
import type { ServiceSection } from '../components/ServicesRail';
import { ServicesSection } from '../components/ServicesSection';
import { CategoriesSection } from '../components/CategoriesSection';
import { GarmentTypesSection } from '../components/GarmentTypesSection';
import { PricesSection } from '../components/PricesSection';
import { DiscountsSection } from '../components/DiscountsSection';

export function ServicesList() {
  const [activeSection, setActiveSection] = useState<ServiceSection>('services');

  const { fetchServices } = useServicesStore();
  const { fetchCategories } = useCategoriesStore();
  const { fetchGarmentTypes } = useGarmentTypesStore();
  const { fetchServiceGarments } = useServiceGarmentsStore();
  const { fetchDiscounts } = useDiscountsStore();

  useEffect(() => {
    fetchServices();
    fetchCategories();
    fetchGarmentTypes();
    fetchServiceGarments();
    fetchDiscounts();
  }, [fetchServices, fetchCategories, fetchGarmentTypes, fetchServiceGarments, fetchDiscounts]);

  return (
    <div className="bg-white border border-zinc-200 rounded-lg overflow-hidden">
      {/* Page header */}
      <div className="px-6 py-5 border-b border-zinc-100">
        <h1 className="text-xl font-semibold text-zinc-900 tracking-tight">
          Gestión de Servicios
        </h1>
      </div>

      {/* Rail + Panel */}
      <div className="flex flex-col lg:flex-row">
        {/* Rail — full width on mobile (Select), sidebar on desktop (nav) */}
        <div className="lg:w-[220px] lg:border-r lg:border-zinc-200 lg:shrink-0">
          <ServicesRail active={activeSection} onChange={setActiveSection} />
        </div>

        {/* Panel */}
        <div className="flex-1 min-w-0">
          {activeSection === 'services' && <ServicesSection />}
          {activeSection === 'categories' && <CategoriesSection />}
          {activeSection === 'garmentTypes' && <GarmentTypesSection />}
          {activeSection === 'prices' && <PricesSection />}
          {activeSection === 'discounts' && <DiscountsSection />}
        </div>
      </div>
    </div>
  );
}
