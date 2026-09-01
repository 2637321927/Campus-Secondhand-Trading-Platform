<template>
  <div class="admin-layout">
    <!-- 左侧导航 -->
    <el-aside width="220px" class="admin-sidebar">
      <div class="logo">
        <h2>🛒 管理后台</h2>
      </div>
      <el-menu
        :default-active="$route.path"
        router
        background-color="#1e2a26"
        text-color="#bfd7d0"
        active-text-color="#24735b"
      >
        <el-menu-item index="/admin/dashboard">
          <el-icon><DataLine /></el-icon>
          <span>数据概览</span>
        </el-menu-item>
        <el-menu-item index="/admin/products/review">
          <el-icon><Document /></el-icon>
          <span>商品审核</span>
        </el-menu-item>
        <el-menu-item index="/admin/products">
          <el-icon><Goods /></el-icon>
          <span>商品管理</span>
        </el-menu-item>
        <el-menu-item index="/admin/users">
          <el-icon><User /></el-icon>
          <span>用户管理</span>
        </el-menu-item>
        <el-menu-item index="/admin/reports">
          <el-icon><Warning /></el-icon>
          <span>举报处理</span>
        </el-menu-item>
        <el-menu-item index="/admin/appeals">
          <el-icon><ChatDotRound /></el-icon>
          <span>申诉处理</span>
        </el-menu-item>
      </el-menu>
    </el-aside>

    <!-- 右侧内容 -->
    <el-container class="admin-main">
      <el-header class="admin-header">
        <div class="header-right">
          <span>{{ authStore.userName || '管理员' }}</span>
          <el-button type="text" @click="handleLogout">退出</el-button>
        </div>
      </el-header>
      <el-main>
        <router-view />
      </el-main>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  DataLine, Document, Goods, User, Warning, ChatDotRound
} from '@element-plus/icons-vue'

const router = useRouter()
const authStore = useAuthStore()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
.admin-layout {
  display: flex;
  height: 100vh;
}
.admin-sidebar {
  background-color: #1e2a26;
  color: white;
  flex-shrink: 0;
}
.logo {
  padding: 20px;
  text-align: center;
  color: white;
  border-bottom: 1px solid rgba(255,255,255,0.1);
}
.logo h2 {
  margin: 0;
  font-size: 18px;
}
.admin-main {
  flex: 1;
  display: flex;
  flex-direction: column;
}
.admin-header {
  background: white;
  border-bottom: 1px solid #e3e9e6;
  display: flex;
  justify-content: flex-end;
  align-items: center;
  padding: 0 20px;
  height: 60px;
}
.header-right {
  display: flex;
  align-items: center;
  gap: 15px;
}
.el-menu {
  border-right: none;
}
.el-main {
  background: #f5f7f6;
  padding: 20px;
}
</style>