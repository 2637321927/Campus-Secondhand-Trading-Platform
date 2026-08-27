import type { RouteRecordRaw } from 'vue-router'

export const adminRoutes: RouteRecordRaw[] = [
  {
    path: '/admin',
    component: () => import('@/layouts/AdminLayout.vue'),
    meta: {
      requiresAuth: true,
      roles: ['admin']
    },
    children: [
      {
        path: 'products/review',
        name: 'ProductReview',
        component: () => import('@/views/admin/product/ProductReview.vue'),
        meta: {
          title: '商品审核'
        }
      }
      // 后续添加更多管理员子路由
    ]
  }
]