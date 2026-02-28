import React, {useState} from 'react';
import {Input} from "../components/ui/input";
import {Button} from "../components/ui/button";

export default function LoginPage() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');

    const handleSubmit = (e: React.SubmitEvent) => {
        e.preventDefault();
        console.log('Username:', username);
        console.log('Password:', password);
    };

    return (
        <div className="flex flex-col items-center justify-center min-h-screen w-screen bg-background">
            <h1 className="text-3xl font-bold mb-8">Marketspace Login</h1>
            <form onSubmit={handleSubmit} className="space-y-6 w-full max-w-sm bg-card p-8 rounded-lg shadow">
                <div className="space-y-2">
                    <label htmlFor="username" className="block text-sm font-medium">Username</label>
                    <Input
                        id="username"
                        type="text"
                        value={username}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setUsername(e.target.value)}
                        required
                        placeholder="Enter your username"
                        autoComplete="username"
                    />
                </div>
                <div className="space-y-2">
                    <label htmlFor="password" className="block text-sm font-medium">Password</label>
                    <Input
                        id="password"
                        type="password"
                        value={password}
                        onChange={(e: React.ChangeEvent<HTMLInputElement>) => setPassword(e.target.value)}
                        required
                        placeholder="Enter your password"
                        autoComplete="current-password"
                    />
                </div>
                <Button type="submit" className="w-full">Login</Button>
            </form>
        </div>
    );
}
