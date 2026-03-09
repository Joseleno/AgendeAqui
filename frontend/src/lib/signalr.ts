import { HubConnectionBuilder, HubConnection, LogLevel } from '@microsoft/signalr'
import { authStore } from '../store/auth-store'

let connection: HubConnection | null = null

export function getConnection(): HubConnection {
  if (connection) return connection

  connection = new HubConnectionBuilder()
    .withUrl('/hubs/appointments', {
      accessTokenFactory: () => authStore.getState().accessToken ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  return connection
}

export async function startConnection() {
  const conn = getConnection()
  if (conn.state === 'Disconnected') {
    await conn.start()
  }
}

export async function stopConnection() {
  if (connection) {
    await connection.stop()
    connection = null
  }
}
