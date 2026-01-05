import React, { useState, useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import Login from './Login';
import TodoList from './TodoList';

function App() {
    const [token, setToken] = useState(localStorage.getItem("token"));

    const updateToken = () => {
        setToken(localStorage.getItem("token"));
    };

    useEffect(() => {
        const handleStorageChange = () => {
            setToken(localStorage.getItem("token"));
        };
        window.addEventListener('storage', handleStorageChange);
        return () => window.removeEventListener('storage', handleStorageChange);
    }, []);

    return (
        <BrowserRouter>
            <Routes>
                <Route path="/login" element={<Login onLogin={updateToken} />} />
                
                <Route 
                    path="/" 
                    element={token ? <TodoList /> : <Navigate to="/login" />} 
                />
                
                <Route path="*" element={<Navigate to="/" />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;