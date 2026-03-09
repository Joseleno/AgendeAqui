import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach } from 'vitest'
import { authStore } from '../store/auth-store'

afterEach(() => {
  cleanup()
  authStore.clear()
  localStorage.clear()
})
