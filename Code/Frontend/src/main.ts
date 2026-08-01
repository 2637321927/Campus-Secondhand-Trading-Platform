import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createPinia } from 'pinia'
import router from './router'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import { useAuthStore } from './stores/auth'
import {AUTH_UNAUTHORIZED_EVENT} from './utils/authEvents'

const app = createApp(App)
const pinia = createPinia()
app.use(pinia)
app.use(router)
app.use(ElementPlus)

const authStore = useAuthStore(pinia)
void authStore.initializeAuth()

window.addEventListener(
    AUTH_UNAUTHORIZED_EVENT,
    () => {
        authStore.clearAuthState()

        const currentRoute =
            router.currentRoute.value

        if (currentRoute.name === 'login') {
            return
        }

        void router.replace({
            name: 'login',
            query: {
                redirect: currentRoute.fullPath
            }
        })
    }
)

app.mount('#app')
