<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { ElMessage } from 'element-plus'
import {
  ArrowDown,
  SwitchButton,
  User
} from '@element-plus/icons-vue'

const keyword = ref('')
const router = useRouter()
const authStore=useAuthStore()

function handleSearch():void{
    const trimmedKeyword=keyword.value.trim()

    router.push({
        path:'/products',
        query:trimmedKeyword
            ?{keyword:trimmedKeyword}
            :{}
    })
}

function goHome(): void {
  router.push('/')
}

function goToLogin(): void {
  router.push('/login')
}

function goToRegister(): void {
  router.push('/register')
}

function goToPublish(): void {
  router.push({
    name: 'product-publish'
  })
}

function goToMyProducts(): void {
  router.push({
    name: 'my-products'
  })
}

async function goFavorites():Promise <void> {
  if(!authStore.isLoggedIn){
    ElMessage.warning('请先登录')

    await router.push({
      name:'login',
      query:{
      redirect:'/user/favorites'
      }
    })

    return

  }

  await router.push({
    name:'my-favorites'
  })
}

function goMessages(): void {
  ElMessage.info('消息功能正在开发中')
}

async function handleUserCommand(command: string): Promise<void> {
  if (command === 'profile') {
    await router.push({ name: 'user-overview' })
    return
  }

  if (command === 'logout') {
    await handleLogout()
  }
}

async function handleLogout(): Promise<void> {
  await authStore.logoutAction().catch((error) => {
    console.error('退出登录失败：', error)
  })

  ElMessage.success('已退出登录')

  await router.push('/')
}
</script>

<template>
  <header class="app-header">
    <div class="header-inner">
      <!-- 平台品牌 -->
      <button
        class="brand"
        type="button"
        aria-label="返回首页"
        @click="goHome"
      >

        <span class="brand-text">
          <strong>校园闲置</strong>
          <small>Campus Market</small>
        </span>
      </button>

      <!-- 全局搜索 -->
      <div class="search-area">
        <el-input
          v-model="keyword"
          class="search-input"
          clearable
          placeholder="搜索商品名称或描述"
          @keyup.enter="handleSearch"
        >
          <template #append>
            <el-button
              class="search-button"
              aria-label="搜索商品"
              @click="handleSearch"
            >
              搜索
            </el-button>
          </template>
        </el-input>
      </div>

      <!-- 导航操作 -->
      <nav class="header-actions" aria-label="用户导航">
        <el-button text @click="goFavorites">
            收藏
        </el-button>

        <el-button text @click="goMessages">
            消息
        </el-button>

        <el-button
          class="publish-button"
          type="primary"
          @click="goToPublish"
        >
          发布闲置
        </el-button>

       <template v-if="!authStore.isLoggedIn">
        <el-button @click="goToLogin">
            登录
        </el-button>

        <el-button text @click="goToRegister">
         注册
        </el-button>
        </template>

        <template v-else>
          <el-button text @click="goToMyProducts">
            我的商品
          </el-button>

          <el-dropdown
            trigger="hover"
            class="user-dropdown"
            @command="handleUserCommand"
          >
            <div class="user-entry">
              <span class="user-entry-name">
                {{ authStore.currentUser?.userName }}
              </span>

              <el-icon class="user-entry-arrow">
                <ArrowDown />
              </el-icon>
            </div>

            <template #dropdown>
              <el-dropdown-menu>
                <el-dropdown-item command="profile">
                  <el-icon><User /></el-icon>
                  个人中心
                </el-dropdown-item>

                <el-dropdown-item
                  command="logout"
                  divided
                >
                  <el-icon><SwitchButton /></el-icon>
                  退出登录
                </el-dropdown-item>
              </el-dropdown-menu>
            </template>
          </el-dropdown>
        </template>

      </nav>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  position: sticky;
  top: 0;
  z-index: 100;

  width: 100%;
  background: rgba(255, 255, 255, 0.96);
  border-bottom: 1px solid #e3e9e6;
  backdrop-filter: blur(12px);
}

.header-inner {
  display: flex;
  width: min(1360px, calc(100% - 40px));
  min-height: 72px;
  margin: 0 auto;
  align-items: center;
  gap: 24px;
}


/* 平台品牌 */
.brand {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  gap: 10px;

  padding: 0;
  color: inherit;
  background: transparent;
  border: none;
  cursor: pointer;
}

.brand:focus-visible {
  outline: 3px solid rgba(36, 115, 91, 0.2);
  outline-offset: 4px;
  border-radius: 12px;
}


.brand-text {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  line-height: 1.2;
}

.brand-text strong {
  color: #1e2a26;
  font-size: 17px;
  font-weight: 700;
  white-space: nowrap;
}

.brand-text small {
  margin-top: 3px;
  color: #6c7a74;
  font-size: 11px;
  letter-spacing: 0.3px;
  white-space: nowrap;
}

.user-dropdown {
  outline: none;
}

.user-entry {
  display: flex;
  max-width: 150px;
  padding: 8px 12px;
  align-items: center;
  gap: 6px;
  overflow: hidden;
  color: #24735b;
  background: #eef7f3;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
}

.user-entry-name {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.user-entry-arrow {
  flex-shrink: 0;
  color: #3e9b79;
  font-size: 12px;
}

/* 搜索区域 */
.search-area {
  flex: 1;
  max-width: 620px;
  min-width: 220px;
}

.search-input {
  width: 100%;
}

.search-area :deep(.el-input__wrapper) {
  min-height: 42px;
  padding-left: 16px;
  background: #f5f7f6;
  border-radius: 12px 0 0 12px;
  box-shadow: 0 0 0 1px #e3e9e6 inset;
}

.search-area :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px #24735b inset;
}

.search-area :deep(.el-input-group__append) {
  padding: 0;
  overflow: hidden;
  background: #24735b;
  border: none;
  border-radius: 0 12px 12px 0;
  box-shadow: none;
}

.search-button {
  min-height: 42px;
  padding: 0 22px;
  color: #ffffff;
  background: #24735b;
  border: none;
  border-radius: 0;
}

.search-button:hover,
.search-button:focus {
  color: #ffffff;
  background: #1d604c;
}

/* 右侧操作 */
.header-actions {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  gap: 4px;
  margin-left: auto;
}

.publish-button {
  min-height: 40px;
  padding: 0 20px;
  border-radius: 10px;
}

</style>
