import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import 'bootstrap/dist/css/bootstrap.min.css';


let connection;

export function createConnection(token) {
    connection = new HubConnectionBuilder()
        .withUrl('https://localhost:5001/hubs/chat', {
            accessTokenFactory: () => token,
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();

    return connection;
}

export function getConnection() {
    return connection;
}
