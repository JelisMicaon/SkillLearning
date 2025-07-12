import { useState, useEffect } from 'react';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import './App.css'; // Opcional, para estilo

function App() {
    const [activities, setActivities] = useState<string[]>([]);
    const [connectionStatus, setConnectionStatus] = useState("Conectando...");

    useEffect(() => {
        const connection = new HubConnectionBuilder()
            .withUrl("/hubs/activity") // O proxy do Vite cuidará disso
            .configureLogging(LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        connection.on("NewUserRegistered", (username) => {
            const message = `✨ Novo usuário registrado: ${username}`;
            setActivities(prev => [message, ...prev]);
        });

        connection.on("UserLoggedIn", (username) => {
            const message = `✅ Usuário logado: ${username}`;
            setActivities(prev => [message, ...prev]);
        });

        const startConnection = async () => {
            try {
                await connection.start();
                setConnectionStatus("Conectado! ✅");
            } catch (err) {
                setConnectionStatus("Falha na conexão. ❌");
                console.error("SignalR Connection Error: ", err);
            }
        };

        startConnection();

        return () => {
            connection.stop();
        };
    }, []);

    return (
        <div className="App">
            <h1>Painel de Atividade em Tempo Real ⚡</h1>
            <p>Status da Conexão SignalR: <strong>{connectionStatus}</strong></p>
            <div className="activity-list">
                {activities.length === 0 && <p>Aguardando atividade no backend...</p>}
                <ul>
                    {activities.map((activity, index) => (
                        <li key={index}>{activity}</li>
                    ))}
                </ul>
            </div>
        </div>
    );
}

export default App;