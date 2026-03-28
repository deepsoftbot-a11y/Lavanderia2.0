import { RouterProvider } from 'react-router-dom';
import { Toaster } from 'sonner';

import { AuthProvider } from '@/shared/components/common/AuthProvider';
import { router } from '@/routes/router';

export function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
      <Toaster position="top-right" richColors />
    </AuthProvider>
  );
}
