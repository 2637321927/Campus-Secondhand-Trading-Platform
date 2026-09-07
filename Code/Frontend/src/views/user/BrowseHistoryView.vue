<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  ElMessage,
  ElMessageBox
} from 'element-plus'
import {
  getMyBrowseHistory,
  clearMyBrowseHistory,
  deleteBrowseHistoryItem
} from '../../api/modules/user'
import type { BrowseHistoryDto } from '../../types/api/user'
import { useProductImages } from '../../composables/useProductImages'

const router = useRouter()

const history = ref<BrowseHistoryDto[]>([])
const loading = ref(false)
const errorMessage = ref('')
const hasLoaded = ref(false)

const clearing = ref(false)
const deletingProductIds = ref<number[]>([])

const {
  loadProductImages,
  getProductImageUrl
} = useProductImages()

let loadVersion = 0

function formatViewTime(value: string): string {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return ''
  }

  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

async function loadHistory(): Promise<void> {
  const currentVersion = ++loadVersion

  loading.value = true
  errorMessage.value = ''

  try {
    const response = await getMyBrowseHistory()

    if (currentVersion !== loadVersion) {
      return
    }

    history.value = response.data ?? []

    await loadProductImages(
      history.value.map((item) => item.productImageFileId)
    ).catch((error) => {
      console.error('浏览历史图片加载失败：', error)
    })
  } catch (error) {
    if (currentVersion !== loadVersion) {
      return
    }

    history.value = []

    errorMessage.value = '浏览历史加载失败，请稍后重试'

    console.error('浏览历史加载失败：', error)
  } finally {
    if (currentVersion === loadVersion) {
      loading.value = false
      hasLoaded.value = true
    }
  }
}

function isDeleting(productId: number): boolean {
  return deletingProductIds.value.includes(productId)
}

async function handleDeleteItem(
  item: BrowseHistoryDto
): Promise<void> {
  if (isDeleting(item.productId)) {
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定从浏览历史中删除「${item.productName}」吗？`,
      '删除浏览记录',
      {
        type: 'warning',
        confirmButtonText: '删除',
        cancelButtonText: '取消'
      }
    )
  } catch {
    return
  }

  deletingProductIds.value = [
    ...deletingProductIds.value,
    item.productId
  ]

  try {
    await deleteBrowseHistoryItem(item.productId)

    history.value = history.value.filter(
      (entry) => entry.productId !== item.productId
    )

    ElMessage.success('已删除浏览记录')
  } catch (error) {
    ElMessage.error('删除失败，请稍后重试')

    console.error('删除浏览记录失败：', error)
  } finally {
    deletingProductIds.value =
      deletingProductIds.value.filter(
        (id) => id !== item.productId
      )
  }
}

async function handleClearAll(): Promise<void> {
  if (history.value.length === 0) {
    return
  }

  try {
    await ElMessageBox.confirm(
      '清空后无法恢复，确定要清空全部浏览历史吗？',
      '清空浏览历史',
      {
        type: 'warning',
        confirmButtonText: '清空',
        cancelButtonText: '取消'
      }
    )
  } catch {
    return
  }

  clearing.value = true

  try {
    await clearMyBrowseHistory()

    history.value = []

    ElMessage.success('已清空浏览历史')
  } catch (error) {
    ElMessage.error('清空失败，请稍后重试')

    console.error('清空浏览历史失败：', error)
  } finally {
    clearing.value = false
  }
}

async function goToProduct(productId: number): Promise<void> {
  await router.push({
    name: 'product-detail',
    params: {
      productId
    }
  })
}

function goBack(): void {
  void router.push({ name: 'user-overview' })
}

onMounted(() => {
  void loadHistory()
})

onBeforeUnmount(() => {
  loadVersion += 1
})
</script>

<template>
  <main class="history-page">
    <div class="history-container">
      <!-- 页面头部 -->
      <header class="page-header">
        <div>
          <p class="page-eyebrow">BROWSE HISTORY</p>

          <h1>浏览历史</h1>

          <p class="page-description">
            查看你最近浏览过的校园二手商品，可单条删除或一键清空。
          </p>
        </div>

        <div class="header-actions">
          <el-button @click="goBack">
            返回个人中心
          </el-button>

          <el-button
            type="danger"
            plain
            :loading="clearing"
            :disabled="
              clearing || history.length === 0
            "
            @click="handleClearAll"
          >
            清空全部
          </el-button>
        </div>
      </header>

      <!-- 加载状态 -->
      <section
        v-if="loading && !hasLoaded"
        class="history-panel"
      >
        <div
          v-for="index in 3"
          :key="index"
          class="history-skeleton"
        >
          <el-skeleton animated>
            <template #template>
              <div class="skeleton-row">
                <el-skeleton-item
                  variant="image"
                  class="skeleton-image"
                />

                <div class="skeleton-body">
                  <el-skeleton-item
                    variant="h3"
                    class="skeleton-title"
                  />

                  <el-skeleton-item
                    variant="text"
                    class="skeleton-text"
                  />
                </div>
              </div>
            </template>
          </el-skeleton>
        </div>
      </section>

      <!-- 错误状态 -->
      <el-result
        v-else-if="errorMessage && history.length === 0"
        icon="error"
        title="浏览历史加载失败"
        :sub-title="errorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            @click="loadHistory"
          >
            重新加载
          </el-button>
        </template>
      </el-result>

      <!-- 空数据状态 -->
      <el-empty
        v-else-if="hasLoaded && history.length === 0"
        description="你还没有浏览记录"
        class="history-empty"
      >
        <el-button
          type="primary"
          plain
          @click="router.push({ name: 'product-list' })"
        >
          去逛逛
        </el-button>
      </el-empty>

      <!-- 浏览历史列表 -->
      <section
        v-else
        class="history-panel"
        v-loading="loading"
      >
        <div class="panel-header">
          <div>
            <h2>最近浏览</h2>

            <span>共 {{ history.length }} 条记录</span>
          </div>

          <el-button
            text
            :loading="loading"
            @click="loadHistory"
          >
            重新加载
          </el-button>
        </div>

        <ul class="history-list">
          <li
            v-for="item in history"
            :key="item.viewId"
            class="history-item"
          >
            <button
              class="history-cover"
              type="button"
              @click="goToProduct(item.productId)"
            >
              <el-image
                v-if="getProductImageUrl(item.productImageFileId)"
                :src="getProductImageUrl(item.productImageFileId)"
                :alt="item.productName"
                fit="cover"
              >
                <template #error>
                  <div class="cover-placeholder">
                    暂无图片
                  </div>
                </template>
              </el-image>

              <div
                v-else
                class="cover-placeholder"
              >
                暂无图片
              </div>
            </button>

            <div class="history-main">
              <div class="history-heading">
                <h3>{{ item.productName }}</h3>

                <span class="history-price">
                  ¥{{ item.productPrice.toFixed(2) }}
                </span>
              </div>

              <time class="history-time">
                {{ formatViewTime(item.viewTime) }}
              </time>
            </div>

            <div class="history-actions">
              <el-button
                type="primary"
                plain
                @click="goToProduct(item.productId)"
              >
                查看商品
              </el-button>

              <el-button
                type="danger"
                link
                :loading="isDeleting(item.productId)"
                :disabled="
                  deletingProductIds.length > 0
                "
                @click="handleDeleteItem(item)"
              >
                删除
              </el-button>
            </div>
          </li>
        </ul>
      </section>
    </div>
  </main>
</template>

<style scoped>
.history-page {
  min-height: calc(100vh - 72px);
  padding: 36px 24px 64px;
  background: #f5f7f6;
  color: #1e2a26;
}

.history-container {
  width: 100%;
  max-width: 900px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 20px;
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

.header-actions {
  display: flex;
  flex-shrink: 0;
  gap: 12px;
}

.history-panel {
  padding: 26px 28px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.panel-header {
  display: flex;
  padding-bottom: 18px;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #edf1ef;
}

.panel-header h2 {
  margin: 0;
  color: #1e2a26;
  font-size: 20px;
}

.panel-header span {
  display: block;
  margin-top: 6px;
  color: #7a8781;
  font-size: 13px;
}

.history-list {
  margin: 0;
  padding: 0;
  list-style: none;
}

.history-item {
  display: flex;
  padding: 18px 0;
  align-items: center;
  gap: 18px;
  border-bottom: 1px solid #edf1ef;
}

.history-item:last-child {
  border-bottom: 0;
}

.history-cover {
  flex: 0 0 96px;
  width: 96px;
  height: 72px;
  padding: 0;
  overflow: hidden;
  background: #eef2f0;
  border: none;
  border-radius: 10px;
  cursor: pointer;
}

.history-cover :deep(.el-image) {
  width: 100%;
  height: 100%;
}

.cover-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  color: #7a8781;
  background: #eef2f0;
  font-size: 12px;
}

.history-main {
  min-width: 0;
  flex: 1;
}

.history-heading {
  display: flex;
  align-items: baseline;
  gap: 14px;
}

.history-heading h3 {
  margin: 0;
  overflow: hidden;
  color: #1e2a26;
  font-size: 16px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.history-price {
  flex-shrink: 0;
  color: #d9544d;
  font-size: 16px;
  font-weight: 700;
}

.history-time {
  display: block;
  margin-top: 8px;
  color: #7a8781;
  font-size: 13px;
}

.history-actions {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  gap: 8px;
}

.history-skeleton {
  padding: 8px 0;
}

.skeleton-row {
  display: flex;
  align-items: center;
  gap: 18px;
}

.skeleton-image {
  width: 96px;
  height: 72px;
  border-radius: 10px;
}

.skeleton-body {
  flex: 1;
}

.skeleton-title {
  width: 40%;
  height: 20px;
}

.skeleton-text {
  width: 28%;
  margin-top: 10px;
}

.history-empty {
  padding: 70px 0 55px;
}

@media (max-width: 640px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .history-item {
    flex-wrap: wrap;
  }

  .history-actions {
    width: 100%;
    justify-content: flex-end;
  }
}
</style>
