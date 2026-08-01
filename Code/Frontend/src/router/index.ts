import {
    createRouter,
    createWebHistory
} from 'vue-router'
import { useAuthStore } from '../stores/auth'

const routes = [
    {
        path: '/login',
        name: 'login',
        component: () =>
            import('../views/auth/LoginView.vue')
    },

    {
        path: '/register',
        name: 'register',
        component: () =>
            import('../views/auth/RegisterView.vue')
    },

    {
        path: '/',
        component: () =>
            import('../layouts/DefaultLayout.vue'),

        children: [
            {
                path: '',
                name: 'home',
                component: () =>
                    import('../views/home/HomeView.vue')
            },

            {
                path: 'user/favorites',
                name: 'my-favorites',
                component: () =>
                    import('../views/user/MyFavoritesView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'products',
                name: 'product-list',
                component: () =>
                    import('../views/product/ProductListView.vue')
            },

            {
                path: 'products/:productId',
                name: 'product-detail',
                component: () =>
                    import('../views/product/ProductDetailView.vue')
            }
        ]
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

router.beforeEach((to) => {
    const authStore = useAuthStore()

    if (
        to.meta.requiresAuth &&
        !authStore.isLoggedIn
    ) {
        return {
            name: 'login',
            query: {
                redirect: to.fullPath
            }
        }
    }

    return true
})

export default router