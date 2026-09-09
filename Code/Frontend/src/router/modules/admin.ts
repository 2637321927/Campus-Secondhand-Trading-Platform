import type { RouteRecordRaw } from 'vue-router'

export const adminRoutes: RouteRecordRaw[] = [
  {
    path: '/admin',
    component: () => import('../../layouts/AdminLayout.vue'),
    meta: {
      requiresAuth: true,
      roles: ['admin']
    },
    redirect: '/admin/dashboard',
    children: [
      // 数据概览
      {
        path: 'dashboard',
        name: 'AdminDashboard',
        component: () => import('../../views/admin/dashboard/DashboardView.vue'),
        meta: { title: '数据概览' }
      },
      // 商品审核
      {
        path: 'products/review',
        name: 'ProductReview',
        component: () => import('../../views/admin/product/ProductReviewView.vue'),
        meta: { title: '商品审核' }
      },
      // 商品管理
      {
        path: 'products',
        name: 'ProductManage',
        component: () => import('../../views/admin/product/ProductManageView.vue'),
        meta: { title: '商品管理' }
      },
      // 用户管理
      {
        path: 'users',
        name: 'UserManage',
        component: () => import('../../views/admin/user/UserManageView.vue'),
        meta: { title: '用户管理' }
      },
      // 用户详情
      {
        path: 'users/:userId',
        name: 'UserDetail',
        component: () => import('../../views/admin/user/UserDetailView.vue'),
        meta: { title: '用户详情' }
      },
      // 订单管理
      {
        path: 'orders',
        name: 'OrderManage',
        component: () => import('../../views/admin/order/OrderManageView.vue'),
        meta: { title: '订单管理' }
      },
      // 工单管理（统一举报与申诉）
      {
        path: 'work-orders',
        name: 'AdminWorkOrderManage',
        component: () => import('../../views/admin/workorder/WorkOrderManageView.vue'),
        meta: { title: '工单管理' }
      },
      // 兼容旧入口
      {
        path: 'appeals',
        redirect: { path: '/admin/work-orders', query: { type: 'appeal' } }
      },
      {
        path: 'reports',
        redirect: { path: '/admin/work-orders', query: { type: 'report' } }
      },
      // 公告管理
      {
        path: 'announcements',
        name: 'AnnouncementManage',
        component: () => import('../../views/admin/announcement/AnnouncementManageView.vue'),
        meta: { title: '公告管理' }
      }
    ]
  }
]
