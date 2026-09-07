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
            },

            /* ===== 模块9: 购买、订单与支付 ===== */
            {
                path: 'user/orders/buying',
                name: 'order-buying',
                component: () =>
                    import('../views/order/BuyingOrderListView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'user/orders/selling',
                name: 'order-selling',
                component: () =>
                    import('../views/order/SellingOrderListView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'orders/:orderId',
                name: 'order-detail',
                component: () =>
                    import('../views/order/OrderDetailView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'products/:productId/purchase',
                name: 'purchase-confirm',
                component: () =>
                    import('../views/order/PurchaseConfirmView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            /* ===== 模块10: 评价与信誉 ===== */
            {
                path: 'reviews/create/:orderId',
                name: 'review-create',
                component: () =>
                    import('../views/review/ReviewCreateView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'products/:productId/reviews',
                name: 'product-reviews',
                component: () =>
                    import('../views/review/ProductReviewListView.vue')
            },

            {
                path: 'users/:userId/reviews',
                name: 'user-reviews',
                component: () =>
                    import('../views/review/UserReviewListView.vue')
            },

            /* ===== 模块11: 举报与违规处理 ===== */
            {
                path: 'reports',
                name: 'report-list',
                component: () =>
                    import('../views/report/ReportListView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'reports/create',
                name: 'report-create',
                component: () =>
                    import('../views/report/ReportCreateView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'reports/:reportId',
                name: 'report-detail',
                component: () =>
                    import('../views/report/ReportDetailView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            /* ===== 模块12: 申诉中心 ===== */
            {
                path: 'appeals',
                name: 'appeal-list',
                component: () =>
                    import('../views/appeal/AppealListView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'appeals/create',
                name: 'appeal-create',
                component: () =>
                    import('../views/appeal/AppealCreateView.vue'),
                meta: {
                    requiresAuth: true
                }
            },

            {
                path: 'appeals/:appealId',
                name: 'appeal-detail',
                component: () =>
                    import('../views/appeal/AppealDetailView.vue'),
                meta: {
                    requiresAuth: true
                }
            }
        ]
    },
    // add routers for admin
    ...adminRoutes
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
