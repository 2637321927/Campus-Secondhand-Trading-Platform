<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
  getMyProfile,
  getMyPublishedProducts,
  getMySoldOrders,
  getMyPurchaseOrders
} from '../../api/modules/user'
import { getCollectionCount } from '../../api/modules/collection'
import type { UserProfileDto } from '../../types/api/user'
import { useAvatarImage } from '../../composables/useAvatarImage'

interface OverviewStats {
  published: number | null
  sold: number | null
  purchased: number | null
  favorites: number | null
}

const router = useRouter()

const profile = ref<UserProfileDto | null>(null)
const loading = ref(false)
const errorMessage = ref('')

const stats = ref<OverviewStats>({
  published: null,
  sold: null,
  purchased: null,
  favorites: null
})

const { avatarUrl, loadAvatar } = useAvatarImage()

const genderText = computed(() => {
  const gender = profile.value?.gender

  if (gender === 'male') {
    return '男'
  }

  if (gender === 'female') {
    return '女'
  }

  return '保密'
})

function formatRegisterTime(value?: string): string {
  if (!value) {
    return ''
  }

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return ''
  }

  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}

async function loadProfile(): Promise<void> {
  loading.value = true
  errorMessage.value = ''

  try {
    const response = await getMyProfile()

    profile.value = response.data

    await loadAvatar(response.data.avatarFileId)
  } catch (error) {
    errorMessage.value = '个人中心资料加载失败，请稍后重试'

    console.error('个人中心资料加载失败：', error)
  } finally {
    loading.value = false
  }
}

async function loadStats(): Promise<void> {
  const [
    publishedResult,
    soldResult,
    purchasedResult,
    favoritesResult
  ] = await Promise.allSettled([
    getMyPublishedProducts(),
    getMySoldOrders(),
    getMyPurchaseOrders(),
    getCollectionCount()
  ])

  if (publishedResult.status === 'fulfilled') {
    stats.value.published =
      (publishedResult.value.data ?? []).length
  }

  if (soldResult.status === 'fulfilled') {
    stats.value.sold =
      (soldResult.value.data ?? []).length
  }

  if (purchasedResult.status === 'fulfilled') {
    stats.value.purchased =
      (purchasedResult.value.data ?? []).length
  }

  if (favoritesResult.status === 'fulfilled') {
    stats.value.favorites =
      favoritesResult.value.data.count
  }
}

function goToProfile(): void {
  void router.push({ name: 'user-profile' })
}

function goToSettings(): void {
  void router.push({ name: 'user-settings' })
}

function goToMyProducts(): void {
  void router.push({ name: 'my-products' })
}

function goToMyFavorites(): void {
  void router.push({ name: 'my-favorites' })
}

function goToHistory(): void {
  void router.push({ name: 'user-history' })
}

function goToAddresses(): void {
  void router.push({ name: 'user-addresses' })
}

function goToReports(): void {
  void router.push({ name: 'user-reports' })
}

function goToSoldOrders(): void {
  ElMessage.info('此功能正在开发中')
}

function goToPurchaseOrders(): void {
  ElMessage.info('此功能正在开发中')
}

onMounted(() => {
  void loadProfile()
  void loadStats()
})
</script>

<template>
  <main class="overview-page">
    <div class="overview-container">
      <!-- 页面头部 -->
      <header class="page-header">
        <p class="page-eyebrow">PERSONAL CENTER</p>

        <h1>个人中心</h1>

        <p class="page-description">
          查看个人资料与账号数据，管理你发布的闲置商品和收藏。
        </p>
      </header>

      <!-- 加载状态 -->
      <section
        v-if="loading"
        class="profile-card"
      >
        <el-skeleton :rows="4" animated />
      </section>

      <!-- 错误状态 -->
      <el-result
        v-else-if="errorMessage"
        icon="error"
        title="个人中心加载失败"
        :sub-title="errorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            @click="loadProfile"
          >
            重新加载
          </el-button>
        </template>
      </el-result>

      <!-- 正常内容 -->
      <template v-else>
        <!-- 个人资料卡片 -->
        <section class="profile-card">
          <div class="profile-main">
            <el-avatar
              class="profile-avatar"
              :size="88"
              :src="avatarUrl"
            >
              {{ profile?.userName?.charAt(0) ?? '用' }}
            </el-avatar>

            <div class="profile-info">
              <div class="profile-heading">
                <h2>{{ profile?.userName }}</h2>

                <el-tag
                  type="success"
                  effect="plain"
                  size="small"
                >
                  {{ genderText }}
                </el-tag>

                <el-tag
                  type="warning"
                  effect="plain"
                  size="small"
                >
                  信誉 {{ profile?.credit ?? 0 }}
                </el-tag>
              </div>

              <p
                v-if="profile?.profile"
                class="profile-bio"
              >
                {{ profile.profile }}
              </p>

              <div class="profile-meta">
                <span>
                  注册于 {{ formatRegisterTime(profile?.registerTime) }}
                </span>
              </div>
            </div>
          </div>

          <div class="profile-actions">
            <el-button
              type="primary"
              @click="goToProfile"
            >
              编辑资料
            </el-button>

            <el-button @click="goToSettings">
              账号设置
            </el-button>
          </div>
        </section>

        <!-- 数据概览 -->
        <section class="stats-section">
          <h3 class="section-title">数据概览</h3>

          <div class="stats-grid">
            <button
              class="stat-card stat-card--link"
              type="button"
              @click="goToMyProducts"
            >
              <span class="stat-value">
                {{ stats.published ?? '—' }}
              </span>

              <span class="stat-label">我发布的商品</span>

              <span class="stat-arrow">查看 →</span>
            </button>

            <button
              class="stat-card stat-card--link"
              type="button"
              @click="goToSoldOrders"
            >
              <span class="stat-value">
                {{ stats.sold ?? '—' }}
              </span>

              <span class="stat-label">我卖出的订单</span>

              <span class="stat-arrow">查看 →</span>
            </button>

            <button
              class="stat-card stat-card--link"
              type="button"
              @click="goToPurchaseOrders"
            >
              <span class="stat-value">
                {{ stats.purchased ?? '—' }}
              </span>

              <span class="stat-label">我购买的订单</span>

              <span class="stat-arrow">查看 →</span>
            </button>

            <button
              class="stat-card stat-card--link"
              type="button"
              @click="goToMyFavorites"
            >
              <span class="stat-value">
                {{ stats.favorites ?? '—' }}
              </span>

              <span class="stat-label">我的收藏</span>

              <span class="stat-arrow">查看 →</span>
            </button>
          </div>
        </section>

        <!-- 快捷入口 -->
        <section class="entries-section">
          <h3 class="section-title">快捷入口</h3>

          <div class="entries-grid">
            <button
              class="entry-card"
              type="button"
              @click="goToMyProducts"
            >
              <span class="entry-title">我的商品</span>

              <span class="entry-desc">
                管理已发布、已售和已下架的商品
              </span>
            </button>

            <button
              class="entry-card"
              type="button"
              @click="goToMyFavorites"
            >
              <span class="entry-title">我的收藏</span>

              <span class="entry-desc">
                查看收藏的校园二手商品
              </span>
            </button>

            <button
              class="entry-card"
              type="button"
              @click="goToProfile"
            >
              <span class="entry-title">个人资料</span>

              <span class="entry-desc">
                修改头像、昵称、性别与个性签名
              </span>
            </button>

            <button
              class="entry-card"
              type="button"
              @click="goToSettings"
            >
              <span class="entry-title">账号设置</span>

              <span class="entry-desc">
                修改登录密码
              </span>
            </button>

            <button
              class="entry-card"
              type="button"
              @click="goToHistory"
            >
              <span class="entry-title">浏览历史</span>

              <span class="entry-desc">
                查看并管理最近浏览过的商品
              </span>
            </button>

            <button
              class="entry-card"
              type="button"
              @click="goToAddresses"
            >
              <span class="entry-title">地址管理</span>

              <span class="entry-desc">
                管理收货与交易地址，设置默认地址
              </span>
            </button>

            <button
              class="entry-card"
              type="button"
              @click="goToReports"
            >
              <span class="entry-title">举报与申诉</span>

              <span class="entry-desc">
                发起举报或申诉，查看处理进度
              </span>
            </button>
          </div>
        </section>
      </template>
    </div>
  </main>
</template>

<style scoped>
.overview-page {
  min-height: calc(100vh - 72px);
  padding: 36px 24px 64px;
  background: #f5f7f6;
  color: #1e2a26;
}

.overview-container {
  width: 100%;
  max-width: 1080px;
  margin: 0 auto;
}

.page-header {
  margin-bottom: 24px;
}

.page-eyebrow {
  margin: 0 0 8px;
  color: #3e9b79;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 1.6px;
}

.page-header h1 {
  margin: 0;
  color: #1e2a26;
  font-size: 30px;
  line-height: 1.25;
}

.page-description {
  margin: 10px 0 0;
  color: #6c7a74;
  font-size: 14px;
  line-height: 1.7;
}

/* 个人资料卡片 */
.profile-card {
  display: flex;
  padding: 30px 32px;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  background:
    linear-gradient(
      135deg,
      #ffffff 0%,
      #edf5f1 100%
    );
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.profile-main {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: 22px;
}

.profile-avatar {
  flex-shrink: 0;
  color: #ffffff;
  background: #3e9b79;
  font-size: 32px;
  font-weight: 700;
}

.profile-info {
  min-width: 0;
}

.profile-heading {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px;
}

.profile-heading h2 {
  margin: 0;
  color: #1e2a26;
  font-size: 26px;
  overflow-wrap: anywhere;
}

.profile-bio {
  margin: 10px 0 0;
  color: #46534d;
  font-size: 14px;
  line-height: 1.7;
}

.profile-meta {
  margin-top: 10px;
  color: #6c7a74;
  font-size: 13px;
}

.profile-actions {
  display: flex;
  flex-shrink: 0;
  gap: 10px;
}

/* 区块标题 */
.section-title {
  margin: 0 0 16px;
  color: #1e2a26;
  font-size: 18px;
}

.stats-section,
.entries-section {
  margin-top: 30px;
}

/* 数据概览 */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 18px;
}

.stat-card {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 8px;
  padding: 24px 22px;
  color: #1e2a26;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  text-align: left;
}

.stat-card--link {
  cursor: pointer;
  transition:
    border-color 0.2s ease,
    box-shadow 0.2s ease;
}

.stat-card--link:hover {
  border-color: #3e9b79;
  box-shadow: 0 8px 20px rgb(36 115 91 / 10%);
}

.stat-value {
  color: #24735b;
  font-size: 32px;
  font-weight: 700;
  line-height: 1.1;
}

.stat-label {
  color: #6c7a74;
  font-size: 14px;
}

.stat-arrow {
  margin-top: 6px;
  color: #3e9b79;
  font-size: 13px;
  font-weight: 600;
}

/* 快捷入口 */
.entries-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 18px;
}

.entry-card {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 8px;
  padding: 22px 24px;
  color: #1e2a26;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  text-align: left;
  cursor: pointer;
  transition:
    border-color 0.2s ease,
    transform 0.2s ease;
}

.entry-card:hover {
  border-color: #3e9b79;
  transform: translateY(-2px);
}

.entry-title {
  font-size: 17px;
  font-weight: 700;
}

.entry-desc {
  color: #6c7a74;
  font-size: 13px;
  line-height: 1.6;
}

@media (max-width: 760px) {
  .profile-card {
    flex-direction: column;
    align-items: flex-start;
  }

  .stats-grid,
  .entries-grid {
    grid-template-columns: 1fr;
  }
}
</style>
