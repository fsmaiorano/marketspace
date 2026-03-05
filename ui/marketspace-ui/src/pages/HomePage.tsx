import {useEffect, useState} from "react";
import {Button} from "@/components/ui/button";
import {Input} from "@/components/ui/input";
import {apiClient} from "@/lib/api";
import {
    getMe,
    getUserType,
    logout,
    type UserType,
} from "@/services/authentication-service";

interface MeResponse {
    userId: string;
    userName: string | null;
    email: string | null;
    firstName: string | null;
    lastName: string | null;
    userType: UserType;
}

interface MerchantFormData {
    name: string;
    description: string;
    address: string;
    phoneNumber: string;
    email: string;
}

const initialMerchantForm: MerchantFormData = {
    name: "",
    description: "",
    address: "",
    phoneNumber: "",
    email: "",
};

export default function HomePage() {
    const [me, setMe] = useState<MeResponse | null>(null);
    const [currentUserType, setCurrentUserType] = useState<UserType | null>(
        getUserType(),
    );
    const [showMerchantForm, setShowMerchantForm] = useState(false);
    const [merchantForm, setMerchantForm] =
        useState<MerchantFormData>(initialMerchantForm);
    const [loadingProfile, setLoadingProfile] = useState(false);
    const [submittingMerchant, setSubmittingMerchant] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    useEffect(() => {
        const loadProfile = async () => {
            setLoadingProfile(true);
            try {
                await fetchMe();
            } catch (err: unknown) {
                const message =
                    err instanceof Error
                        ? err.message
                        : "Failed to load profile on mount.";
                setError(message);
            } finally {
                setLoadingProfile(false);
            }
        };

        loadProfile();
    }, [currentUserType === null]);

    const fetchMe = async (): Promise<MeResponse> => {
        const profile = await getMe();
        setMe(profile);

        setCurrentUserType(profile.userType);

        setMerchantForm((prev) => ({
            ...prev,
            email: prev.email || profile.email || "",
        }));

        return profile;
    };

    const handleProfileClick = async () => {
        setError(null);
        setSuccess(null);
        setLoadingProfile(true);

        try {
            await fetchMe();
        } catch (err: unknown) {
            const message =
                err instanceof Error ? err.message : "Failed to load profile.";
            setError(message);
        } finally {
            setLoadingProfile(false);
        }
    };

    const handleLogout = () => {
        logout();
        window.location.href = "/";
    };

    const handleBecomeMerchant = async () => {
        setError(null);
        setSuccess(null);
        setShowMerchantForm(true);

        if (!me) {
            try {
                await fetchMe();
            } catch {
                // The form can still be filled manually when profile fetch fails.
            }
        }
    };

    const handleMerchantInputChange = (
        field: keyof MerchantFormData,
        value: string,
    ) => {
        setMerchantForm((prev) => ({
            ...prev,
            [field]: value,
        }));
    };

    const handleMerchantSubmit = async (
        event: React.FormEvent<HTMLFormElement>,
    ) => {
        event.preventDefault();
        setError(null);
        setSuccess(null);
        setSubmittingMerchant(true);

        try {
            const profile = me ?? (await fetchMe());

            await apiClient.post("/api/merchant", {
                Name: merchantForm.name,
                Description: merchantForm.description,
                Address: merchantForm.address,
                PhoneNumber: merchantForm.phoneNumber,
                Email: merchantForm.email,
            });

            await apiClient.put("/api/user/update-user-type", {
                UserId: profile.userId,
                UserType: 1,
            });

            localStorage.setItem("userType", "Merchant");
            setCurrentUserType("Merchant");
            setSuccess(
                "Merchant profile created successfully. Your account is now Merchant.",
            );
            setShowMerchantForm(false);
            setMerchantForm(initialMerchantForm);

            window.location.href = "/merchant";
        } catch (err: unknown) {
            const message =
                err instanceof Error
                    ? err.message
                    : "Failed to create merchant profile.";
            setError(message);
        } finally {
            setSubmittingMerchant(false);
        }
    };

    const alreadyMerchant = currentUserType === "Merchant";

    return (
        <div className="min-h-screen bg-background">
            <header className="border-b bg-card">
                <div className="mx-auto flex w-full max-w-5xl items-center justify-between px-4 py-3">
                    <div>
                        <h1 className="text-xl font-bold">MarketSpace</h1>
                        <p className="text-sm text-muted-foreground">
                            Welcome to MarketSpace!
                        </p>
                    </div>

                    <div className="flex items-center gap-2">
                        <Button
                            variant="outline"
                            onClick={handleProfileClick}
                            disabled={loadingProfile || submittingMerchant}
                        >
                            {loadingProfile ? "Loading..." : "Profile (Me)"}
                        </Button>
                        {!alreadyMerchant && (
                            <Button
                                onClick={handleBecomeMerchant}
                                disabled={submittingMerchant}
                            >
                                Become a Merchant
                            </Button>
                        )}
                        <Button
                            variant="destructive"
                            onClick={handleLogout}
                            disabled={submittingMerchant}
                        >
                            Logout
                        </Button>
                    </div>
                </div>
            </header>

            <main className="mx-auto w-full max-w-5xl space-y-6 px-4 py-6">
                {error && (
                    <div className="rounded-md border border-red-300 bg-red-50 px-4 py-3 text-sm text-red-700">
                        {error}
                    </div>
                )}

                {success && (
                    <div className="rounded-md border border-green-300 bg-green-50 px-4 py-3 text-sm text-green-700">
                        {success}
                    </div>
                )}

                {me && (
                    <section className="rounded-lg border bg-card p-4">
                        <h2 className="mb-2 text-lg font-semibold">My Profile</h2>
                        <p className="text-sm">
                            <strong>User ID:</strong> {me.userId}
                        </p>
                        <p className="text-sm">
                            <strong>User Name:</strong> {me.userName || "-"}
                        </p>
                        <p className="text-sm">
                            <strong>Email:</strong> {me.email || "-"}
                        </p>
                        <p className="text-sm">
                            <strong>Type:</strong> {me.userType}
                        </p>
                    </section>
                )}

                {showMerchantForm && !alreadyMerchant && (
                    <section className="rounded-lg border bg-card p-4">
                        <h2 className="mb-4 text-lg font-semibold">Become a Merchant</h2>
                        <form className="space-y-4" onSubmit={handleMerchantSubmit}>
                            <div className="space-y-1">
                                <label htmlFor="name" className="text-sm font-medium">
                                    Business Name
                                </label>
                                <Input
                                    id="name"
                                    value={merchantForm.name}
                                    onChange={(event) =>
                                        handleMerchantInputChange("name", event.target.value)
                                    }
                                    placeholder="Your store name"
                                    required
                                    disabled={submittingMerchant}
                                />
                            </div>

                            <div className="space-y-1">
                                <label htmlFor="description" className="text-sm font-medium">
                                    Description
                                </label>
                                <Input
                                    id="description"
                                    value={merchantForm.description}
                                    onChange={(event) =>
                                        handleMerchantInputChange("description", event.target.value)
                                    }
                                    placeholder="What do you sell?"
                                    required
                                    disabled={submittingMerchant}
                                />
                            </div>

                            <div className="space-y-1">
                                <label htmlFor="address" className="text-sm font-medium">
                                    Address
                                </label>
                                <Input
                                    id="address"
                                    value={merchantForm.address}
                                    onChange={(event) =>
                                        handleMerchantInputChange("address", event.target.value)
                                    }
                                    placeholder="Business address"
                                    required
                                    disabled={submittingMerchant}
                                />
                            </div>

                            <div className="space-y-1">
                                <label htmlFor="phoneNumber" className="text-sm font-medium">
                                    Phone Number
                                </label>
                                <Input
                                    id="phoneNumber"
                                    value={merchantForm.phoneNumber}
                                    onChange={(event) =>
                                        handleMerchantInputChange("phoneNumber", event.target.value)
                                    }
                                    placeholder="Contact phone"
                                    required
                                    disabled={submittingMerchant}
                                />
                            </div>

                            <div className="space-y-1">
                                <label htmlFor="email" className="text-sm font-medium">
                                    Business Email
                                </label>
                                <Input
                                    id="email"
                                    type="email"
                                    value={merchantForm.email}
                                    onChange={(event) =>
                                        handleMerchantInputChange("email", event.target.value)
                                    }
                                    placeholder="Business email"
                                    required
                                    disabled={submittingMerchant}
                                />
                            </div>

                            <div className="flex items-center gap-2 pt-2">
                                <Button type="submit" disabled={submittingMerchant}>
                                    {submittingMerchant ? "Creating..." : "Create Merchant"}
                                </Button>
                                <Button
                                    type="button"
                                    variant="outline"
                                    disabled={submittingMerchant}
                                    onClick={() => setShowMerchantForm(false)}
                                >
                                    Cancel
                                </Button>
                            </div>
                        </form>
                    </section>
                )}
            </main>
        </div>
    );
}
