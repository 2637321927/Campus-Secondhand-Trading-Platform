import {
    createRouter,
    createWebHistory
} from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { adminRoutes} from './modules/admin'
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
                path: 'user/products',
                name: 'my-products',
                component: () =>
                    import('../views/user/MyProductsView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'user',
                name: 'user-overview',
                component: () =>
                    import('../views/user/UserOverviewView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'user/profile',
                name: 'user-profile',
                component: () =>
                    import('../views/user/ProfileView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'user/settings',
                name: 'user-settings',
                component: () =>
                    import('../views/user/AccountSettingView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'user/history',
                name: 'user-history',
                component: () =>
                    import('../views/user/BrowseHistoryView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'user/addresses',
                name: 'user-addresses',
                component: () =>
                    import('../views/user/AddressView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'users/:userId',
                name: 'user-home',
                component: () =>
                    import('../views/user/UserHomeView.vue')
            },

            {
                path: 'messages',
                name: 'message-list',
                component: () =>
                    import('../views/message/MessageListView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'messages/:conversationId',
                name: 'message-chat',
                component: () =>
                    import('../views/message/ChatView.vue'),
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
                path: 'products/publish',
                name: 'product-publish',
                component: () =>
                    import('../views/product/ProductPublishView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'products/:productId/edit',
                name: 'product-edit',
                component: () =>
                    import('../views/product/ProductEditView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'seller/products/:productId',
                name: 'seller-product-detail',
                component: () =>
                    import('../views/seller/SellerProductDetailView.vue'),
                meta: {
                    requiresAuth: true
                }
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
