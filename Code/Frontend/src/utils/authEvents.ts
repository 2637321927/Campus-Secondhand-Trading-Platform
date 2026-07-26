export const AUTH_UNAUTHORIZED_EVENT =
    'auth:unauthorized'

export function notifyUnauthorized(): void {
    window.dispatchEvent(
        new Event(AUTH_UNAUTHORIZED_EVENT)
    )
}