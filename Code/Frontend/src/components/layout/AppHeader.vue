<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { ElMessage } from 'element-plus'

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
  ElMessage.info('发布商品功能正在开发中')
}

function goFavorites(): void {
  ElMessage.info('收藏功能正在开发中')
}

function goMessages(): void {
  ElMessage.info('消息功能正在开发中')
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

        <div v-else class="user-entry">
        {{ authStore.currentUser?.userName }}
        </div>

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

.user-entry {
  max-width: 120px;
  padding: 8px 12px;
  overflow: hidden;
  color: #24735b;
  background: #eef7f3;
  border-radius: 10px;
  font-weight: 600;
  text-overflow: ellipsis;
  white-space: nowrap;
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