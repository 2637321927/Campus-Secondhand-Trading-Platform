<script setup lang="ts">
import {
  onBeforeUnmount,
  onMounted,
  ref
} from 'vue'
import { useRouter } from 'vue-router'
import { getSellerProducts } from '../../api/modules/seller'
import type {
  ProductDto,
  ProductStatus
} from '../../types/api/product'
import type {
  SellerProductQuery
} from '../../types/api/seller'
import { resolveFileUrl } from '../../utils/image'
import { formatDate } from '../../utils/format'
import SellerProductActions from '../../components/product/SellerProductActions.vue'

const router = useRouter()

const products = ref<ProductDto[]>([])
const loading = ref(false)
const errorMessage = ref('')
const hasLoaded = ref(false)

const keyword = ref('')
const selectedStatus = ref<'all' | ProductStatus>('all')

let loadVersion = 0

function getStatusText(status: ProductStatus): string {
  if (status === 0) {
    return '在售'
  }

  if (status === 1) {
    return '已售'
  }

  if (status === 2) {
    return '已下架'
  }

  return '草稿'
}

function getStatusType(
  status: ProductStatus
): 'success' | 'warning' | 'info' | 'danger' {
  if (status === 0) {
    return 'success'
  }

  if (status === 1) {
    return 'info'
  }

  if (status === 2) {
    return 'danger'
  }

  return 'warning'
}

function getCoverUrl(product: ProductDto): string {
  const images = [...(product.images ?? [])]

  images.sort((a, b) => {
    return a.imgIndex - b.imgIndex
  })

  const firstImage = images[0]

  if (!firstImage) {
    return ''
  }

  return resolveFileUrl(firstImage.imgFileId)
}

function getReleaseDate(product: ProductDto): string {
  if (!product.releaseDate) {
    return ''
  }

  return formatDate(product.releaseDate)
}

function createQuery(): SellerProductQuery {
  const query: SellerProductQuery = {}
  const trimmedKeyword = keyword.value.trim()

  if (trimmedKeyword) {
    query.keyword = trimmedKeyword
  }

  if (selectedStatus.value !== 'all') {
    query.status = selectedStatus.value
  }

  return query
}

async function loadProducts(): Promise<void> {
  const currentVersion = ++loadVersion

  loading.value = true
  errorMessage.value = ''

  try {
    const response = await getSellerProducts(
      createQuery()
    )

    if (currentVersion !== loadVersion) {
      return
    }

    products.value = response.data ?? []
  } catch (error) {
    if (currentVersion !== loadVersion) {
      return
    }

    errorMessage.value =
      '我的商品加载失败，请确认后端接口是否可用后重试'

    console.error('我的商品加载失败：', error)
  } finally {
    if (currentVersion === loadVersion) {
      loading.value = false
      hasLoaded.value = true
    }
  }
}

async function handleSearch(): Promise<void> {
  await loadProducts()
}

async function handleReset(): Promise<void> {
  keyword.value = ''
  selectedStatus.value = 'all'
  await loadProducts()
}

async function goToManage(productId: number): Promise<void> {
  await router.push({
    name: 'seller-product-detail',
    params: {
      productId
    }
  })
}

async function goToEdit(productId: number): Promise<void> {
  await router.push({
    name: 'product-edit',
    params: {
      productId
    }
  })
}

async function goToPublish(): Promise<void> {
  await router.push({
    name: 'product-publish'
  })
}

function handleActionFinished(): void {
  void loadProducts()
}

function handleActionDeleted(): void {
  void loadProducts()
}

onMounted(() => {
  void loadProducts()
})

onBeforeUnmount(() => {
  loadVersion += 1
})
</script>

<template>
  <main class="my-products-page">
    <section class="my-products-container">
      <header class="page-header">
        <div>
          <h1>我的商品</h1>
          <p>管理已发布、已售、下架和草稿商品。</p>
        </div>

        <el-button
          type="primary"
          @click="goToPublish"
        >
          发布闲置
        </el-button>
      </header>

      <el-card
        class="filter-card"
        shadow="never"
      >
        <div class="filter-row">
          <el-input
            v-model="keyword"
            clearable
            placeholder="搜索商品名称"
            class="keyword-input"
            @keyup.enter="handleSearch"
          />

          <el-select
            v-model="selectedStatus"
            class="status-select"
            @change="loadProducts"
          >
            <el-option label="全部状态" value="all" />
            <el-option label="在售" :value="0" />
            <el-option label="已售" :value="1" />
            <el-option label="已下架" :value="2" />
            <el-option label="草稿" :value="3" />
          </el-select>

          <el-button
            type="primary"
            :loading="loading"
            @click="handleSearch"
          >
            搜索
          </el-button>

          <el-button
            :disabled="loading"
            @click="handleReset"
          >
            重置
          </el-button>
        </div>
      </el-card>

      <el-alert
        v-if="errorMessage && products.length > 0"
        :title="errorMessage"
        type="error"
        :closable="false"
        show-icon
        class="page-alert"
      >
        <template #default>
          当前仍保留上一次成功加载的商品列表。
        </template>
      </el-alert>

      <div
        v-if="loading && !hasLoaded"
        class="page-state"
      >
        <el-skeleton :rows="5" animated />
      </div>

      <el-result
        v-else-if="errorMessage && products.length === 0"
        icon="error"
        title="商品加载失败"
        :sub-title="errorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            :loading="loading"
            @click="loadProducts"
          >
            重新加载
          </el-button>
        </template>
      </el-result>

      <el-empty
        v-else-if="hasLoaded && products.length === 0"
        description="当前条件下没有商品"
      >
        <el-button
          type="primary"
          @click="goToPublish"
        >
          发布第一件商品
        </el-button>
      </el-empty>

      <div
        v-else
        class="product-list"
        v-loading="loading"
      >
        <el-card
          v-for="product in products"
          :key="product.productId"
          class="product-item"
          shadow="never"
        >
          <div class="product-item-layout">
            <div class="product-image">
              <el-image
                v-if="getCoverUrl(product)"
                :src="getCoverUrl(product)"
                :alt="product.name"
                fit="cover"
              >
                <template #error>
                  <div class="image-placeholder">
                    图片加载失败
                  </div>
                </template>
              </el-image>

              <div
                v-else
                class="image-placeholder"
              >
                暂无图片
              </div>
            </div>

            <div class="product-main">
              <div class="product-heading">
                <div>
                  <h2>{{ product.name }}</h2>

                  <p class="product-price">
                    ¥{{ product.price.toFixed(2) }}
                  </p>
                </div>

                <el-tag
                  :type="getStatusType(product.status)"
                  effect="light"
                >
                  {{ getStatusText(product.status) }}
                </el-tag>
              </div>

              <div class="product-meta">
                <span v-if="product.categoryName">
                  分类：{{ product.categoryName }}
                </span>

                <span v-if="getReleaseDate(product)">
                  发布时间：{{ getReleaseDate(product) }}
                </span>

                <span>
                  浏览量：{{ product.viewCount }}
                </span>
              </div>

              <p
                v-if="product.info"
                class="product-info"
              >
                {{ product.info }}
              </p>

              <div class="product-toolbar">
                <div class="navigation-actions">
                  <el-button
                    type="primary"
                    @click="goToManage(product.productId)"
                  >
                    管理详情
                  </el-button>

                  <el-button
                    @click="goToEdit(product.productId)"
                  >
                    编辑
                  </el-button>
                </div>

                <SellerProductActions
                  :product-id="product.productId"
                  :status="product.status"
                  :disabled="loading"
                  @changed="handleActionFinished"
                  @deleted="handleActionDeleted"
                />
              </div>
            </div>
          </div>
        </el-card>
      </div>
    </section>
  </main>
</template>

<style scoped>
.my-products-page {
  min-height: calc(100vh - 72px);
  padding: 32px 20px 56px;
  background: #f5f7f6;
  color: #1e2a26;
}

.my-products-container {
  width: min(1180px, 100%);
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  margin-bottom: 24px;
  text-align: left;
}

.page-header h1 {
  margin: 0;
  color: #1e2a26;
  font-size: 30px;
}

.page-header p {
  margin: 8px 0 0;
  color: #6c7a74;
}

.filter-card,
.product-item {
  border: 1px solid #e3e9e6;
  border-radius: 14px;
}

.filter-row {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
}

.keyword-input {
  width: min(420px, 100%);
}

.status-select {
  width: 170px;
}

.page-alert {
  margin-top: 18px;
}

.page-state {
  margin-top: 20px;
  padding: 32px;
  background: #fff;
  border-radius: 14px;
}

.product-list {
  display: grid;
  gap: 16px;
  margin-top: 20px;
}

.product-item-layout {
  display: grid;
  grid-template-columns: 180px minmax(0, 1fr);
  gap: 22px;
}

.product-image {
  overflow: hidden;
  aspect-ratio: 4 / 3;
  border-radius: 12px;
  background: #eef2f0;
}

.product-image :deep(.el-image) {
  width: 100%;
  height: 100%;
}

.image-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  color: #7a8781;
  background: #eef2f0;
}

.product-main {
  min-width: 0;
  text-align: left;
}

.product-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 16px;
}

.product-heading h2 {
  margin: 0;
  color: #1e2a26;
  font-size: 20px;
  overflow-wrap: anywhere;
}

.product-price {
  margin-top: 8px;
  color: #d9544d;
  font-size: 22px;
  font-weight: 700;
}

.product-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 8px 18px;
  margin-top: 14px;
  color: #6c7a74;
  font-size: 13px;
}

.product-info {
  display: -webkit-box;
  margin-top: 12px;
  overflow: hidden;
  color: #46534d;
  line-height: 1.7;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

.product-toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-top: 20px;
}

.navigation-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.navigation-actions :deep(.el-button + .el-button) {
  margin-left: 0;
}

</style>
