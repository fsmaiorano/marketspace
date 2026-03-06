import { getUserType } from "@/services/authentication-service";
import CustomerPage from "@/pages/customer/CustomerPage";
import MerchantPage from "@/pages/merchant/MerchantPage";

export default function HomePage() {
  const userType = getUserType();

  if (userType === "Merchant") {
    return <MerchantPage />;
  }

  return <CustomerPage />;
}
