import { Outlet } from '@tanstack/react-router';
import { Toaster } from 'sonner';

function App() {
  return (
    <>
      <Outlet />
      <Toaster richColors position="top-right" />
    </>
  );
}

export default App;
