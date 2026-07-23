import {
    createRouter,
    createWebHistory
} from 'vue-router'

const routes = [
    {
        path: '/login',
        name: 'login',
        component: () => import('../views/auth/LoginView.vue')
    },

    {
        path:'/register',
        name:'register',
        component:()=>import('../views/auth/RegisterView.vue')
    },
        
    {
        path: '/',
        component: () => import('../layouts/DefaultLayout.vue'),
        children: [
            {
                path: '',
                name: 'home',
                component: () => import('../views/home/HomeView.vue')
            },

            {
                path: 'products/:productId',
                name: 'product-detail',
                component: () =>
                    import('../views/product/ProductDetailView.vue')
            },

            {
                path: 'products',
                name: 'product-list',
                component: () =>
                    import('../views/product/ProductListView.vue')
            }
        ]
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})


export default router