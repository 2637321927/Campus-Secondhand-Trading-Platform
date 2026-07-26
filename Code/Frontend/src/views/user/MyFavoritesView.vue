<script setup lang="ts">
import { onMounted, ref,onBeforeUnmount,computed } from 'vue'
import { useRouter } from 'vue-router'
import ProductCard from '../../components/product/ProductCard.vue'
import {
  batchDeleteCollections,
  getCollectionCount,
  getCollections,
  searchCollections,
  toggleCollection
} from '../../api/modules/collection'
import type {
  ProductCardDto
} from '../../types/api/product'
import {
  ElMessage,
  ElMessageBox
} from 'element-plus'
import type {
  CheckboxValueType
} from 'element-plus'

const router = useRouter()

const favorites = ref<ProductCardDto[]>([])
const collectionCount = ref(0)

const loading = ref(false)
const errorMessage = ref('')

const searchInput = ref('')
const activeKeyword = ref('')

// 当前页面选中的商品 ID
const selectedProductIds = ref<number[]>([])

// 正在单独取消收藏的商品 ID
const cancellingProductIds = ref<number[]>([])

// 是否正在批量取消
const batchDeleting = ref(false)

const collectionOperationLocked = ref(false)

let favoritesLoadVersion = 0

const selectedCount = computed(() => {
  return selectedProductIds.value.length
})

const allCurrentSelected = computed(() => {
  if (favorites.value.length === 0) {
    return false
  }

  return favorites.value.every((product) =>
    selectedProductIds.value.includes(
      product.productId
    )
  )
})

const selectionIndeterminate = computed(() => {
  return (
    selectedProductIds.value.length > 0 &&
    !allCurrentSelected.value
  )
})

function isCurrentFavoritesLoad(
  version: number
): boolean {
  return version === favoritesLoadVersion
}

function isProductCancelling(
  productId: number
): boolean {
  return cancellingProductIds.value.includes(
    productId
  )
}

function setProductCancelling(
  productId: number,
  cancelling: boolean
): void {
  if (cancelling) {
    if (
      !cancellingProductIds.value.includes(
        productId
      )
    ) {
      cancellingProductIds.value = [
        ...cancellingProductIds.value,
        productId
      ]
    }

    return
  }

  cancellingProductIds.value =
    cancellingProductIds.value.filter(
      (id) => id !== productId
    )
}

function handleProductSelection(
  productId: number,
  checkedValue: CheckboxValueType
): void {
  if (collectionOperationLocked.value) {
    return
  }

  const checked = Boolean(checkedValue)

  if (checked) {
    if (
      !selectedProductIds.value.includes(
        productId
      )
    ) {
      selectedProductIds.value = [
        ...selectedProductIds.value,
        productId
      ]
    }

    return
  }

  selectedProductIds.value =
    selectedProductIds.value.filter(
      (id) => id !== productId
    )
}

function handleToggleSelectAll(
  checkedValue: CheckboxValueType
): void {
  if (collectionOperationLocked.value) {
    return
  }

  const checked = Boolean(checkedValue)

  if (!checked) {
    selectedProductIds.value = []
    return
  }

  selectedProductIds.value =
    favorites.value.map(
      (product) => product.productId
    )
}

async function loadFavoritesPage(): Promise<void> {
  const currentVersion =
    ++favoritesLoadVersion

  loading.value = true
  errorMessage.value = ''

  try {
    const [
      collectionResult,
      countResult
    ] = await Promise.allSettled([
      getCollections(),
      getCollectionCount()
    ])

    if (
      !isCurrentFavoritesLoad(currentVersion)
    ) {
      return
    }

    if (collectionResult.status === 'rejected') {
      throw collectionResult.reason
    }

    favorites.value =
      collectionResult.value.data ?? []

    selectedProductIds.value = []

    activeKeyword.value = ''

    if (countResult.status === 'fulfilled') {
      const count =
        countResult.value.data.count

      collectionCount.value =
        Number.isInteger(count) && count >= 0
          ? count
          : favorites.value.length
    } else {
      collectionCount.value =
        favorites.value.length

      console.error(
        '收藏数量加载失败：',
        countResult.reason
      )
    }
  } catch (error) {
    if (
      !isCurrentFavoritesLoad(currentVersion)
    ) {
      return
    }

    favorites.value = []
    collectionCount.value = 0

    errorMessage.value =
      '收藏列表加载失败，请稍后重试'

    console.error(
      '收藏列表加载失败：',
      error
    )
  } finally {
    if (
      isCurrentFavoritesLoad(currentVersion)
    ) {
      loading.value = false
    }
  }
}

async function loadSearchResults(
  keyword: string
): Promise<void> {
  const currentVersion =
    ++favoritesLoadVersion

  loading.value = true
  errorMessage.value = ''

  try {
    const response =
      await searchCollections(keyword)

    if (
      !isCurrentFavoritesLoad(currentVersion)
    ) {
      return
    }

    favorites.value = response.data ?? []
    selectedProductIds.value = []
    activeKeyword.value = keyword
  } catch (error) {
    if (
      !isCurrentFavoritesLoad(currentVersion)
    ) {
      return
    }

    favorites.value = []

    errorMessage.value =
      '收藏搜索失败，请稍后重试'

    console.error(
      '收藏搜索失败：',
      error
    )
  } finally {
    if (
      isCurrentFavoritesLoad(currentVersion)
    ) {
      loading.value = false
    }
  }
}

async function handleClearSearch():
Promise<void> {
  searchInput.value = ''
  activeKeyword.value = ''

  await loadFavoritesPage()
}

async function handleSearch(): Promise<void> {
  const keyword = searchInput.value.trim()

  if (!keyword) {
    await handleClearSearch()
    return
  }

  await loadSearchResults(keyword)
}

async function handleRefresh(): Promise<void> {
  if (activeKeyword.value) {
    await loadSearchResults(
      activeKeyword.value
    )

    return
  }

  await loadFavoritesPage()
}

async function handleBrowseProducts(): Promise<void> {
  await router.push({
    name: 'product-list'
  })
}

async function refreshCollectionCount():
Promise<void> {
  try {
    const response =
      await getCollectionCount()

    const count = response.data.count

    if (
      Number.isInteger(count) &&
      count >= 0
    ) {
      collectionCount.value = count
    }
  } catch (error) {
    console.error(
      '收藏数量刷新失败：',
      error
    )
  }
}

async function handleCancelFavorite(
  product: ProductCardDto
): Promise<void> {
  if (
    loading.value ||
    collectionOperationLocked.value
  ) {
    return
  }

  // 必须在弹出确认框之前上锁
  collectionOperationLocked.value = true

  try {
    await ElMessageBox.confirm(
      `确定取消收藏“${product.name}”吗？`,
      '取消收藏',
      {
        type: 'warning',
        confirmButtonText: '确定取消',
        cancelButtonText: '保留收藏'
      }
    )
  } catch {
    collectionOperationLocked.value = false
    return
  }

  setProductCancelling(
    product.productId,
    true
  )

  try {
    const response = await toggleCollection(
      product.productId
    )

    if (response.data.isCollected) {
      ElMessage.warning(
        '收藏状态未成功取消，请重新加载后重试'
      )

      await handleRefresh()
      return
    }

    favorites.value = favorites.value.filter(
      (item) =>
        item.productId !== product.productId
    )

    selectedProductIds.value =
      selectedProductIds.value.filter(
        (id) => id !== product.productId
      )

    collectionCount.value = Math.max(
      0,
      collectionCount.value - 1
    )

    ElMessage.success('已取消收藏')

    void refreshCollectionCount()
  } catch (error) {
    ElMessage.error(
      '取消收藏失败，请稍后重试'
    )

    console.error(
      '取消收藏失败：',
      error
    )
  } finally {
    setProductCancelling(
      product.productId,
      false
    )

    collectionOperationLocked.value = false
  }
}

async function handleBatchCancelFavorites():
Promise<void> {
  if (
    loading.value ||
    collectionOperationLocked.value
  ) {
    return
  }

  const productIds = [
    ...selectedProductIds.value
  ]

  if (productIds.length === 0) {
    ElMessage.warning(
      '请先选择需要取消收藏的商品'
    )

    return
  }

  // 必须在确认框出现前锁定单个取消操作
  collectionOperationLocked.value = true

  try {
    await ElMessageBox.confirm(
      `确定取消收藏选中的 ${productIds.length} 件商品吗？`,
      '批量取消收藏',
      {
        type: 'warning',
        confirmButtonText: '确定取消',
        cancelButtonText: '暂不取消'
      }
    )
  } catch {
    collectionOperationLocked.value = false
    return
  }

  batchDeleting.value = true

  try {
    const response =
      await batchDeleteCollections(productIds)

    const deleted = response.data.deleted

    ElMessage.success(
      `已取消收藏 ${deleted} 件商品`
    )

    selectedProductIds.value = []

    await handleRefresh()
    await refreshCollectionCount()
  } catch (error) {
    ElMessage.error(
      '批量取消收藏失败，请稍后重试'
    )

    console.error(
      '批量取消收藏失败：',
      error
    )
  } finally {
    batchDeleting.value = false
    collectionOperationLocked.value = false
  }
}

onMounted(() => {
  void loadFavoritesPage()
})

onBeforeUnmount(() => {
  favoritesLoadVersion += 1
})
</script>
<template>
  <div class="favorites-page">
    <div class="favorites-container">
      <!-- 页面头部 -->
      <header class="page-header">
        <div>
          <p class="page-eyebrow">
            MY COLLECTIONS
          </p>

          <h1>我的收藏</h1>

          <p class="page-description">
            管理你收藏的校园二手商品，快速查看商品的最新状态。
          </p>
        </div>

        <div class="header-actions">
          <el-tag
            type="success"
            effect="plain"
            size="large"
          >
            共 {{ collectionCount }} 件
          </el-tag>

          <el-button
            type="primary"
            :disabled="collectionOperationLocked"
            @click="handleBrowseProducts"
          >
            继续浏览商品
          </el-button>
        </div>
      </header>

      <!-- 收藏内容区域 -->
      <section class="favorites-panel">
        <!-- 面板标题 -->
        <div class="panel-header">
          <div>
            <h2>收藏商品</h2>

            <span v-if="activeKeyword">
              搜索“{{ activeKeyword }}”，找到
              {{ favorites.length }} 件商品
            </span>

            <span v-else>
              这里展示你当前收藏的全部商品
            </span>
          </div>

          <el-button
            text
            :loading="loading"
            :disabled="collectionOperationLocked"
            @click="handleRefresh"
          >
            重新加载
          </el-button>
        </div>

        <!-- 搜索与批量操作栏 -->
        <div class="favorites-toolbar">
          <div class="search-actions">
            <el-input
              v-model="searchInput"
              clearable
              maxlength="50"
              placeholder="搜索收藏中的商品"
              class="favorites-search"
              :disabled="
                loading ||
                collectionOperationLocked
              "
              @keyup.enter="handleSearch"
              @clear="handleClearSearch"
            >
              <template #append>
                <el-button
                  :loading="loading"
                  :disabled="collectionOperationLocked"
                  @click="handleSearch"
                >
                  搜索
                </el-button>
              </template>
            </el-input>

            <el-button
              v-if="activeKeyword"
              :disabled="
                loading ||
                collectionOperationLocked
              "
              @click="handleClearSearch"
            >
              清除搜索
            </el-button>
          </div>

          <div class="batch-actions">
            <el-checkbox
              :model-value="allCurrentSelected"
              :indeterminate="selectionIndeterminate"
              :disabled="
                loading ||
                collectionOperationLocked ||
                favorites.length === 0
              "
              @change="handleToggleSelectAll"
            >
              全选当前结果
            </el-checkbox>

            <el-button
              type="danger"
              plain
              :disabled="
                selectedCount === 0 ||
                loading ||
                collectionOperationLocked
              "
              :loading="batchDeleting"
              @click="handleBatchCancelFavorites"
            >
              批量取消收藏

              <span v-if="selectedCount > 0">
                （{{ selectedCount }}）
              </span>
            </el-button>
          </div>
        </div>

        <!-- 加载状态 -->
        <div
          v-if="loading"
          class="favorites-loading"
        >
          <div
            v-for="index in 4"
            :key="index"
            class="skeleton-card"
          >
            <el-skeleton animated>
              <template #template>
                <el-skeleton-item
                  variant="image"
                  class="skeleton-image"
                />

                <el-skeleton-item
                  variant="h3"
                  class="skeleton-title"
                />

                <el-skeleton-item
                  variant="text"
                  class="skeleton-text"
                />

                <el-skeleton-item
                  variant="text"
                  class="skeleton-price"
                />
              </template>
            </el-skeleton>
          </div>
        </div>

        <!-- 错误状态 -->
        <el-result
          v-else-if="errorMessage"
          icon="error"
          title="收藏列表加载失败"
          :sub-title="errorMessage"
          class="favorites-result"
        >
          <template #extra>
            <el-button
              type="primary"
              :disabled="collectionOperationLocked"
              @click="handleRefresh"
            >
              重新加载
            </el-button>
          </template>
        </el-result>

        <!-- 空数据状态 -->
        <el-empty
          v-else-if="favorites.length === 0"
          :description="
            activeKeyword
              ? `没有找到与“${activeKeyword}”相关的收藏商品`
              : '你还没有收藏任何商品'
          "
          class="favorites-empty"
        >
          <el-button
            v-if="activeKeyword"
            type="primary"
            plain
            :disabled="collectionOperationLocked"
            @click="handleClearSearch"
          >
            查看全部收藏
          </el-button>

          <el-button
            v-else
            type="primary"
            plain
            :disabled="collectionOperationLocked"
            @click="handleBrowseProducts"
          >
            去逛逛
          </el-button>
        </el-empty>

        <!-- 收藏商品列表 -->
        <div
          v-else
          class="favorites-grid"
        >
          <div
            v-for="product in favorites"
            :key="product.productId"
            class="favorite-card-item"
          >
            <!-- 商品操作栏 -->
            <div
              class="favorite-card-toolbar"
              @click.stop
            >
              <el-checkbox
                :model-value="
                  selectedProductIds.includes(
                    product.productId
                  )
                "
                :disabled="collectionOperationLocked"
                @change="
                  handleProductSelection(
                    product.productId,
                    $event
                  )
                "
              >
                选择
              </el-checkbox>

              <el-button
                type="danger"
                link
                :disabled="
                  collectionOperationLocked &&
                  !isProductCancelling(
                    product.productId
                  )
                "
                :loading="
                  isProductCancelling(
                    product.productId
                  )
                "
                @click.stop="
                  handleCancelFavorite(product)
                "
              >
                取消收藏
              </el-button>
            </div>

            <!-- ProductCard 内部包含进入商品详情的逻辑 -->
            <ProductCard
              :product="product"
            />
          </div>
        </div>
      </section>
    </div>
  </div>
</template>

<style scoped>
.favorites-page {
  min-height: calc(100vh - 72px);
  padding: 36px 24px 60px;
  background: #f5f7f6;
}

.favorites-container {
  width: 100%;
  max-width: 1280px;
  margin: 0 auto;
}

.page-header {
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
  font-size: 32px;
  line-height: 1.25;
}

.page-description {
  margin: 10px 0 0;
  color: #6c7a74;
  font-size: 14px;
  line-height: 1.7;
}

.favorites-panel {
  margin-top: 24px;
  padding: 28px 30px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.panel-header {
  display: flex;
  padding-bottom: 20px;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #edf1ef;
}

.panel-header h2 {
  margin: 0;
  color: #1e2a26;
  font-size: 22px;
}

.panel-header span {
  display: block;
  margin-top: 6px;
  color: #7a8781;
  font-size: 13px;
}

.favorites-empty {
  padding: 70px 0 55px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 14px;
}

.favorites-loading,
.favorites-grid {
  display: grid;
  margin-top: 26px;
  grid-template-columns:
    repeat(4, minmax(0, 1fr));
  gap: 22px;
}

.skeleton-card {
  overflow: hidden;
  padding: 14px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
}

.skeleton-image {
  width: 100%;
  height: 190px;
  border-radius: 12px;
}

.skeleton-title {
  width: 72%;
  height: 22px;
  margin-top: 18px;
}

.skeleton-text {
  width: 54%;
  margin-top: 12px;
}

.skeleton-price {
  width: 35%;
  height: 20px;
  margin-top: 15px;
}

.favorites-result {
  padding: 48px 0 32px;
}

.favorites-toolbar {
  display: flex;
  margin-top: 22px;
  align-items: center;
  gap: 12px;
}

.favorites-search {
  width: 100%;
  max-width: 520px;
}

.favorites-toolbar {
  display: flex;
  margin-top: 22px;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
}

.search-actions {
  display: flex;
  min-width: 0;
  flex: 1;
  align-items: center;
  gap: 12px;
}

.favorites-search {
  width: 100%;
  max-width: 520px;
}

.batch-actions {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  gap: 14px;
}

.favorite-card-item {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 8px;
}

.favorite-card-toolbar {
  display: flex;
  padding: 8px 11px;
  align-items: center;
  justify-content: space-between;
  background: #f7faf8;
  border: 1px solid #e3e9e6;
  border-radius: 12px;
}

.favorite-card-toolbar :deep(.el-checkbox) {
  height: auto;
}
</style>