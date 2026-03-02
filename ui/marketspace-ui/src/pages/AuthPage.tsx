import React, {useState} from 'react';
import {Input} from '../components/ui/input';
import {Button} from '../components/ui/button';
import {
    login,
    register,
    type UserType,
    type LoginRequest,
    type RegisterRequest
} from '../services/authentication-service';
import {envConfig} from '../lib/env-config';

type AuthMode = 'login' | 'register';

export default function AuthPage() {
    const [mode, setMode] = useState<AuthMode>('login');
    const [userType, setUserType] = useState<UserType>('Customer');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [name, setName] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);

    const handleLogin = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setSuccess(null);
        setLoading(true);

        try {
            // Convert userType to number (Customer = 0, Merchant = 1)
            const userTypeNumber = userType === 'Customer' ? 0 : 1;

            const credentials: LoginRequest = {
                email,
                password,
                userType: userTypeNumber,
            };

            const response = await login(credentials);
            
            localStorage.setItem('refreshToken', response.refreshToken);
            localStorage.setItem('userType', userType);
            
            setSuccess(`Welcome back, ${email}!`);

            setTimeout(() => {
                window.location.href = '/home';
            }, 1500);
        } catch (err: unknown) {
            const errorMessage = err instanceof Error ? err.message : 'Login failed. Please try again.';
            setError(errorMessage);
            console.error('Login error:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleRegister = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setSuccess(null);

        if (password !== confirmPassword) {
            setError('Passwords do not match');
            return;
        }

        if (password.length < 6) {
            setError('Password must be at least 6 characters long');
            return;
        }

        setLoading(true);

        try {
            // Convert userType to number (Customer = 0, Merchant = 1)
            const userTypeNumber = userType === 'Customer' ? 0 : 1;

            const data: RegisterRequest = {
                email: email,
                password: password,
                name: name,
                username: email, 
                userType: userTypeNumber,
            };

            const response = await register(data);
            
            localStorage.setItem('refreshToken', response.refreshToken);
            
            setSuccess(`Account created successfully! Welcome, ${name || email}`);

            setEmail('');
            setPassword('');
            setConfirmPassword('');
            setName('');

            setTimeout(() => {
                window.location.href = '/';
            }, 1500);
        } catch (err: unknown) {
            const errorMessage = err instanceof Error ? err.message : 'Registration failed. Please try again.';
            setError(errorMessage);
            console.error('Registration error:', err);
        } finally {
            setLoading(false);
        }
    };

    const handleSubmit = (e: React.FormEvent) => {
        if (mode === 'login') {
            handleLogin(e);
        } else {
            handleRegister(e);
        }
    };

    return (
        <div className="flex flex-col items-center justify-center min-h-screen w-screen bg-background">
            <div className="mb-4 text-sm text-gray-500">
                Environment: <span className="font-semibold">{envConfig.env}</span> | API: <span
                className="font-semibold">{envConfig.bffApiUrl}</span>
            </div>

            <h1 className="text-3xl font-bold mb-2">MarketSpace</h1>
            <p className="text-gray-600 mb-8">{mode === 'login' ? 'Login to your account' : 'Create a new account'}</p>

            <form onSubmit={handleSubmit} className="space-y-6 w-full max-w-sm bg-card p-8 rounded-lg shadow-lg">
                {/* Error Message */}
                {error && (
                    <div className="p-4 bg-red-100 border border-red-400 text-red-700 rounded-md text-sm">
                        {error}
                    </div>
                )}

                {/* Success Message */}
                {success && (
                    <div className="p-4 bg-green-100 border border-green-400 text-green-700 rounded-md text-sm">
                        {success}
                    </div>
                )}

                {/* User Type Selection */}
                <div className="space-y-3">
                    <label className="block text-sm font-medium">I am a:</label>
                    <div className="flex gap-4">
                        <label className="flex items-center gap-2 cursor-pointer">
                            <input
                                type="radio"
                                name="userType"
                                value="Customer"
                                checked={userType === 'Customer'}
                                onChange={(e) => setUserType(e.target.value as UserType)}
                                disabled={loading}
                                className="w-4 h-4"
                            />
                            <span className="text-sm">Customer</span>
                        </label>
                        <label className="flex items-center gap-2 cursor-pointer">
                            <input
                                type="radio"
                                name="userType"
                                value="Merchant"
                                checked={userType === 'Merchant'}
                                onChange={(e) => setUserType(e.target.value as UserType)}
                                disabled={loading}
                                className="w-4 h-4"
                            />
                            <span className="text-sm">Merchant</span>
                        </label>
                    </div>
                </div>

                {/* Email Field */}
                <div className="space-y-2">
                    <label htmlFor="email" className="block text-sm font-medium">
                        Email
                    </label>
                    <Input
                        id="email"
                        type="email"
                        value={email}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setEmail(e.target.value)}
                        required
                        placeholder="Enter your email"
                        autoComplete="email"
                        disabled={loading}
                    />
                </div>

                {/* Name Field (Register Only) */}
                {mode === 'register' && (
                    <div className="space-y-2">
                        <label htmlFor="name" className="block text-sm font-medium">
                            Full Name
                        </label>
                        <Input
                            id="name"
                            type="text"
                            value={name}
                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setName(e.target.value)}
                            required
                            placeholder="Enter your full name"
                            autoComplete="name"
                            disabled={loading}
                        />
                    </div>
                )}

                {/* Password Field */}
                <div className="space-y-2">
                    <label htmlFor="password" className="block text-sm font-medium">
                        Password
                    </label>
                    <Input
                        id="password"
                        type="password"
                        value={password}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
                        required
                        placeholder="Enter your password"
                        autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
                        disabled={loading}
                    />
                </div>

                {/* Confirm Password Field (Register Only) */}
                {mode === 'register' && (
                    <div className="space-y-2">
                        <label htmlFor="confirmPassword" className="block text-sm font-medium">
                            Confirm Password
                        </label>
                        <Input
                            id="confirmPassword"
                            type="password"
                            value={confirmPassword}
                            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setConfirmPassword(e.target.value)}
                            required
                            placeholder="Confirm your password"
                            autoComplete="new-password"
                            disabled={loading}
                        />
                    </div>
                )}

                {/* Submit Button */}
                <Button
                    type="submit"
                    className="w-full"
                    disabled={loading}
                >
                    {loading ? 'Loading...' : (mode === 'login' ? 'Login' : 'Create Account')}
                </Button>

                {/* Toggle Mode */}
                <div className="text-center text-sm">
                    {mode === 'login' ? (
                        <>
                            Don't have an account?{' '}
                            <button
                                type="button"
                                onClick={() => {
                                    setMode('register');
                                    setError(null);
                                    setSuccess(null);
                                }}
                                className="text-blue-600 hover:underline font-medium"
                                disabled={loading}
                            >
                                Register here
                            </button>
                        </>
                    ) : (
                        <>
                            Already have an account?{' '}
                            <button
                                type="button"
                                onClick={() => {
                                    setMode('login');
                                    setError(null);
                                    setSuccess(null);
                                }}
                                className="text-blue-600 hover:underline font-medium"
                                disabled={loading}
                            >
                                Login here
                            </button>
                        </>
                    )}
                </div>
            </form>
        </div>
    );
}


