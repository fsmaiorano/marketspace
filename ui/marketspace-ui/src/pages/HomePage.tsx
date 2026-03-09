import { useEffect, useState } from "react";
import {
  getMe,
  getUserType,
  normalizeUserType,
  storeUserType,
  type UserType,
} from "@/services/authentication-service";
import CustomerPage from "@/pages/customer/CustomerPage";
import MerchantPage from "@/pages/merchant/MerchantPage";
import { useAuthStore } from "@/stores/auth-store";

export default function HomePage() {
  const [userType, setUserType] = useState<UserType | null>(getUserType());
  const [isResolving, setIsResolving] = useState(true);
  const { setUser } = useAuthStore();

  useEffect(() => {
    let active = true;

    const resolveUserType = async () => {
      try {
        const me = await getMe();
        const resolvedType = normalizeUserType(me.userType) ?? getUserType() ?? "Customer";
        storeUserType(resolvedType);
        setUser({
          id: me.userId,
          email: me.email ?? me.userName ?? "",
          userType: resolvedType,
        });

        if (active) {
          setUserType(resolvedType);
        }
      } catch {
        if (active) {
          setUserType(getUserType());
        }
      } finally {
        if (active) {
          setIsResolving(false);
        }
      }
    };

    void resolveUserType();

    return () => {
      active = false;
    };
  }, [setUser]);

  if (isResolving) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-background text-sm text-muted-foreground">
        Loading your workspace...
      </div>
    );
  }

  if (userType === "Merchant") {
    return <MerchantPage />;
  }

  return <CustomerPage />;
}
