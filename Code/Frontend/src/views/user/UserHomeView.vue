<script setup lang="ts">
import {
  computed,
  onMounted,
  ref
} from 'vue'
import {
  useRoute,
  useRouter
} from 'vue-router'
import {
  getUserById,
  getUserProducts,
  getUserSoldProducts,
  searchUserProducts
} from '../../api/modules/user'
import type { UserDto } from '../../types/api/user'
import type {
  ProductDto,
  ProductImageDto,
  ProductCardDto
} from '../../types/api/product'
import { useProductImages } from '../../composables/useProductImages'
import { useAuthStore } from '../../stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const user = ref<UserDto | null>(null)
const loading = ref(false)
const errorMessage = ref('')

const activeTab = ref<string>('published')

const publishedProducts = ref<ProductDto[]>([])
const soldProducts = ref<ProductDto[]>([])
const productsLoading = ref(false)

const searchKeyword = ref('')
const searchedKeyword = ref('')
const searchResults = ref<ProductCardDto[]>([])
const searching = ref(false)
const searchErrorMessage = ref('')

const {
  loadProductImages,
  getProductImageUrl
} = useProductImages()

const userId = computed<number | null>(() => {
  const value = route.params.userId

  if (typeof value !== 'string') {
    return null
  }

  const id = Number(value)

  if (!Number.isInteger(id) || id <= 0) {
    return null
  }

  return id
})

const currentProducts = computed<ProductDto[]>(() => {
  if (activeTab.value === 'sold') {
    return soldProducts.value
  }

  return publishedProducts.value
})

const isCurrentUser = computed(() =>
  user.value?.userId === authStore.currentUser?.userId
)

const isSearchMode = computed(() =>
  searchedKeyword.value !== ''
)

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

function getCoverImage(product: ProductDto): ProductImageDto | undefined {
  return [...(product.images ?? [])].sort(
    (a, b) => a.imgIndex - b.imgIndex
  )[0]
}

function getCoverImageUrl(product: ProductDto): string {
  const cover = getCoverImage(product)

  return getProductImageUrl(cover?.imgFileId)
}

function getSearchCoverUrl(product: ProductCardDto): string {
  return getProductImageUrl(product.coverImageFileId)
}

async function handleSearch(): Promise<void> {
  const keyword = searchKeyword.value.trim()

  if (!keyword || userId.value === null) {
    return
  }

  searching.value = true
  searchErrorMessage.value = ''

  try {
    const response = await searchUserProducts(userId.value, {
      keyword,
      page: 1,
      pageSize: 50,
      sortBy: 'relevance'
    })

    searchedKeyword.value = keyword
    searchResults.value = response.data.items ?? []

    await loadProductImages(
      searchResults.value.map(
        (product) => product.coverImageFileId ?? null
      )
    ).catch((error) => {
      console.error('搜索结果图片加载失败：', error)
    })
  } catch (error) {
    searchErrorMessage.value = '搜索失败，请稍后重试'

    console.error('用户商品搜索失败：', error)
  } finally {
    searching.value = false
  }
}

function handleClearSearch(): void {
  searchKeyword.value = ''
  searchedKeyword.value = ''
  searchResults.value = []
  searchErrorMessage.value = ''
}

async function loadUser(): Promise<void> {
  loading.value = true
  errorMessage.value = ''

  if (userId.value === null) {
    errorMessage.value = '用户编号不正确'
    loading.value = false
    return
  }

  try {
    const response = await getUserById(userId.value)

    user.value = response.data
  } catch (error) {
    errorMessage.value = '用户主页加载失败，请稍后重试'

    console.error('用户主页加载失败：', error)
  } finally {
    loading.value = false
  }
}

async function loadProducts(): Promise<void> {
  if (userId.value === null) {
    return
  }

  productsLoading.value = true

  const [
    publishedResult,
    soldResult
  ] = await Promise.allSettled([
    getUserProducts(userId.value),
    getUserSoldProducts(userId.value)
  ])

  if (publishedResult.status === 'fulfilled') {
    publishedProducts.value = publishedResult.value.data ?? []
  }

  if (soldResult.status === 'fulfilled') {
    soldProducts.value = soldResult.value.data ?? []
  }

  productsLoading.value = false

  const allProducts = [
    ...publishedProducts.value,
    ...soldProducts.value
  ]

  await loadProductImages(
    allProducts.map((product) => getCoverImage(product)?.imgFileId)
  ).catch((error) => {
    console.error('商品图片加载失败：', error)
  })
}

function goBack(): void {
  void router.back()
}

function goToProduct(productId: number): void {
  void router.push({
    name: 'product-detail',
    params: {
      productId
    }
  })
}

function goToUserReport(): void {
  if (userId.value === null) {
    return
  }

  void router.push({
    name: 'report-create',
    query: {
      type: 'user',
      id: String(userId.value)
    }
  })
}

onMounted(() => {
  void loadUser()
  void loadProducts()
})
</script>

<template>
  <main class="user-home-page">
    <div class="user-home-container">
      <!-- 返回按钮 -->
      <div class="back-row">
        <el-button text @click="goBack">
          ← 返回上一页
        </el-button>
      </div>

      <!-- 加载状态 -->
      <section
        v-if="loading"
        class="profile-card"
      >
        <el-skeleton :rows="3" animated />
      </section>

      <!-- 错误状态 -->
      <el-result
        v-else-if="errorMessage"
        icon="error"
        title="用户主页加载失败"
        :sub-title="errorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            @click="loadUser"
          >
            重新加载
          </el-button>
        </template>
      </el-result>

      <!-- 用户主页内容 -->
      <template v-else>
        <!-- 用户资料卡片 -->
        <section class="profile-card">
          <el-avatar
            class="profile-avatar"
            :size="80"
          >
            {{ user?.userName?.charAt(0) ?? '用' }}
          </el-avatar>

          <div class="profile-info">
            <div class="profile-heading">
              <h1>{{ user?.userName ?? '未知用户' }}</h1>

              <el-tag
                v-if="isCurrentUser"
                type="success"
                effect="plain"
                size="small"
              >
                这是你自己
              </el-tag>
            </div>

            <div class="profile-meta">
              <span>用户编号：{{ user?.userId ?? '—' }}</span>

              <span
                v-if="user?.registerTime"
                class="meta-divider"
              >
                ·
              </span>

              <span>
                注册于 {{ formatRegisterTime(user?.registerTime) }}
              </span>
            </div>
          </div>

          <el-button
            v-if="!isCurrentUser"
            type="danger"
            plain
            @click="goToUserReport"
          >
            举报用户
          </el-button>
        </section>

        <!-- 商品列表 -->
        <section class="products-section">
          <!-- 站内搜索 -->
          <div class="user-search">
            <el-input
              v-model="searchKeyword"
              class="user-search-input"
              clearable
              placeholder="在该用户发布的商品中搜索"
              :disabled="searching"
              @keyup.enter="handleSearch"
              @clear="handleClearSearch"
            >
              <template #append>
                <el-button
                  :loading="searching"
                  @click="handleSearch"
                >
                  搜索
                </el-button>
              </template>
            </el-input>
          </div>

          <!-- 搜索结果 -->
          <template v-if="isSearchMode">
            <div class="search-result-header">
              <span>
                搜索“{{ searchedKeyword }}”的结果，共
                {{ searchResults.length }} 件
              </span>

              <el-button
                text
                type="primary"
                @click="handleClearSearch"
              >
                清除搜索
              </el-button>
            </div>

            <!-- 搜索失败 -->
            <el-result
              v-if="searchErrorMessage"
              icon="error"
              title="搜索失败"
              :sub-title="searchErrorMessage"
            />

            <!-- 搜索空状态 -->
            <el-empty
              v-else-if="searchResults.length === 0"
              description="没有找到符合条件的商品"
            />

            <!-- 搜索结果网格 -->
            <div
              v-else
              class="products-grid"
            >
              <button
                v-for="product in searchResults"
                :key="product.productId"
                class="product-card"
                type="button"
                @click="goToProduct(product.productId)"
              >
                <div class="product-cover">
                  <el-image
                    v-if="getSearchCoverUrl(product)"
                    :src="getSearchCoverUrl(product)"
                    :alt="product.name"
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
                </div>

                <div class="product-body">
                  <h3>{{ product.name }}</h3>

                  <div class="product-footer">
                    <span class="product-price">
                      ¥{{ product.price.toFixed(2) }}
                    </span>
                  </div>
                </div>
              </button>
            </div>
          </template>

          <!-- 常规 Tab 列表 -->
          <template v-else>
            <el-tabs v-model="activeTab">
              <el-tab-pane
                label="在售商品"
                name="published"
              />

              <el-tab-pane
                label="已卖出"
                name="sold"
              />
            </el-tabs>

            <!-- 商品加载中 -->
            <div
              v-if="productsLoading"
              class="products-loading"
            >
              <el-skeleton
                :rows="4"
                animated
              />
            </div>

            <!-- 空状态 -->
            <el-empty
              v-else-if="currentProducts.length === 0"
              :description="
                activeTab === 'sold'
                  ? '该用户还没有已卖出的商品'
                  : '该用户还没有在售商品'
              "
            />

            <!-- 商品网格 -->
            <div
              v-else
              class="products-grid"
            >
              <button
                v-for="product in currentProducts"
                :key="product.productId"
                class="product-card"
                type="button"
                @click="goToProduct(product.productId)"
              >
                <div class="product-cover">
                  <el-image
                    v-if="getCoverImageUrl(product)"
                    :src="getCoverImageUrl(product)"
                    :alt="product.name"
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
                </div>

                <div class="product-body">
                  <h3>{{ product.name }}</h3>

                  <div class="product-footer">
                    <span class="product-price">
                      ¥{{ product.price.toFixed(2) }}
                    </span>

                    <el-tag
                      :type="product.status === 0 ? 'success' : 'info'"
                      size="small"
                      effect="plain"
                    >
                      {{ product.status === 0 ? '在售' : '已售' }}
                    </el-tag>
                  </div>
                </div>
              </button>
            </div>
          </template>
        </section>
      </template>
    </div>
  </main>
</template>

<style scoped>
.user-home-page {
  min-height: calc(100vh - 72px);
  padding: 32px 24px 64px;
  background: #f5f7f6;
  color: #1e2a26;
}

.user-home-container {
  width: 100%;
  max-width: 1080px;
  margin: 0 auto;
}

.back-row {
  margin-bottom: 18px;
}

.back-row :deep(.el-button) {
  padding-left: 0;
  color: #537168;
}

/* 用户资料卡片 */
.profile-card {
  display: flex;
  padding: 30px 32px;
  align-items: center;
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

.profile-avatar {
  flex-shrink: 0;
  color: #ffffff;
  background: #3e9b79;
  font-size: 30px;
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

.profile-heading h1 {
  margin: 0;
  color: #1e2a26;
  font-size: 26px;
  overflow-wrap: anywhere;
}

.profile-meta {
  display: flex;
  flex-wrap: wrap;
  margin-top: 12px;
  align-items: center;
  gap: 8px;
  color: #6c7a74;
  font-size: 13px;
}

.meta-divider {
  color: #c0c9c5;
}

/* 商品列表 */
.products-section {
  margin-top: 30px;
}

.products-section :deep(.el-tabs__item) {
  font-size: 15px;
}

.user-search {
  margin-bottom: 20px;
}

.user-search-input {
  max-width: 460px;
}

.user-search-input :deep(.el-input__wrapper) {
  min-height: 42px;
  background: #ffffff;
  border-radius: 10px 0 0 10px;
  box-shadow: 0 0 0 1px #e3e9e6 inset;
}

.user-search-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px #24735b inset;
}

.user-search-input :deep(.el-input-group__append) {
  padding: 0;
  overflow: hidden;
  background: #24735b;
  border: none;
  border-radius: 0 10px 10px 0;
  box-shadow: none;
}

.user-search-input :deep(.el-input-group__append .el-button) {
  min-height: 42px;
  padding: 0 20px;
  color: #ffffff;
  background: #24735b;
  border: none;
  border-radius: 0;
}

.user-search-input
  :deep(.el-input-group__append .el-button:hover) {
  color: #ffffff;
  background: #1d604c;
}

.search-result-header {
  display: flex;
  margin-bottom: 18px;
  padding: 14px 18px;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  color: #65736d;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 14px;
  font-size: 14px;
}

.products-loading {
  margin-top: 20px;
}

.products-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 18px;
  margin-top: 20px;
}

.product-card {
  display: flex;
  padding: 0;
  overflow: hidden;
  flex-direction: column;
  color: #1e2a26;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  text-align: left;
  cursor: pointer;
  transition:
    border-color 0.2s ease,
    transform 0.2s ease,
    box-shadow 0.2s ease;
}

.product-card:hover {
  border-color: #3e9b79;
  transform: translateY(-3px);
  box-shadow: 0 10px 24px rgb(36 115 91 / 10%);
}

.product-cover {
  width: 100%;
  aspect-ratio: 1 / 1;
  overflow: hidden;
  background: #eef2f0;
}

.product-cover :deep(.el-image) {
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

.product-body {
  display: flex;
  padding: 14px 16px 16px;
  flex: 1;
  flex-direction: column;
  gap: 12px;
}

.product-body h3 {
  margin: 0;
  overflow: hidden;
  color: #1e2a26;
  font-size: 15px;
  line-height: 1.45;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-footer {
  display: flex;
  margin-top: auto;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.product-price {
  color: #d9544d;
  font-size: 16px;
  font-weight: 700;
}

@media (max-width: 900px) {
  .products-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 560px) {
  .profile-card {
    flex-direction: column;
    align-items: flex-start;
  }

  .products-grid {
    grid-template-columns: 1fr;
  }
}
</style>
