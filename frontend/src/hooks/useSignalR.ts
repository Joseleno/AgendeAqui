import { useEffect, useRef } from 'react'
import { getConnection, startConnection, stopConnection } from '../lib/signalr'
import { useAuthState } from './useAuth'

export function useSignalR(
  eventName: string,
  handler: (...args: unknown[]) => void,
) {
  const { isAuthenticated } = useAuthState()
  const handlerRef = useRef(handler)
  handlerRef.current = handler

  useEffect(() => {
    if (!isAuthenticated) return

    const conn = getConnection()
    const wrappedHandler = (...args: unknown[]) => handlerRef.current(...args)

    conn.on(eventName, wrappedHandler)
    startConnection().catch(console.error)

    return () => {
      conn.off(eventName, wrappedHandler)
    }
  }, [eventName, isAuthenticated])
}

export function useSignalRCleanup() {
  useEffect(() => {
    return () => {
      stopConnection().catch(console.error)
    }
  }, [])
}
