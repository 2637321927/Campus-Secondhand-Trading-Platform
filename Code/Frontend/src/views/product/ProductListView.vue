<script setup lang="ts">
import { computed, onMounted, ref,watch } from 'vue'
import { useRoute } from 'vue-router'
import { getProducts } from '../../api/modules/product'
import type { 
    ProductDto,
    ProductStatus
 } from '../../types/api/product'
import ProductListCard from '../../components/product/ProductListCard.vue'
import { getCategoryProducts } from '../../api/modules/category'
import { Loading } from '@element-plus/icons-vue'

type SortOption =
  | 'default'
  | 'price-asc'
  | 'price-desc'
  | 'views-desc'

const loading = ref(false)
const errorMessage = ref('')
const products = ref<ProductDto[]>([])
const selectedStatus = ref<'all' | ProductStatus>('all')
const minPrice = ref<number | null>(null)
const maxPrice = ref<number | null>(null)
const sortOption = ref<SortOption>('default')

const route=useRoute()

const categoryId=computed<number|null>(()=>{
    const value=route.query.categoryId

    if(typeof value !=='string'){
        return null
    }

    const id=Number(value)

    if(!Number.isInteger(id)||id<=0){
        return null
    }

    return id
})

const priceRangeError = computed(() => {
  return (
    minPrice.value !== null &&
    maxPrice.value !== null &&
    minPrice.value > maxPrice.value
  )
})

async function loadProducts():Promise<void> {
    loading.value=true
    errorMessage.value=''
    products.value=[]

    try{
        if(categoryId.value){
            const response=await getCategoryProducts(categoryId.value)
            products.value = response.data
        }
        else{
            const response=await getProducts()
            products.value = response.data
        }
    }
    catch(error){
        errorMessage.value='商品列表加载失败'
        console.error('商品列表加载失败：', error)
    }
    finally{
        loading.value=false
    }
}

onMounted(() => {
  loadProducts()
})

watch(
  () => route.query.categoryId,
  () => {
    loadProducts()
  }
)

const keyword = computed(() => {
  const value = route.query.keyword

  return typeof value === 'string'
    ? value.trim().toLowerCase()
    : ''
})

const displayedProducts = computed(() => {
  const normalizedKeyword = keyword.value.toLowerCase()

  let result = products.value.filter((product) => {
    // 关键词条件
        const matchesKeyword =
        !normalizedKeyword ||
        product.name.toLowerCase().includes(normalizedKeyword) ||
        (product.info?.toLowerCase().includes(normalizedKeyword) ?? false)
    // 状态条件
        const matchesStatus =
        selectedStatus.value === 'all' ||
        product.status === selectedStatus.value
    // 最低价格条件
        const matchesMinPrice =
        minPrice.value === null ||
        product.price >= minPrice.value
    // 最高价格条件
        const matchesMaxPrice =
        maxPrice.value === null ||
        product.price <= maxPrice.value

        return (
        matchesKeyword &&
        matchesStatus &&
        matchesMinPrice &&
        matchesMaxPrice
        )
  })

  result = [...result]

    if (sortOption.value === 'price-asc') {
        result.sort((a, b) => a.price - b.price)
    }

    if (sortOption.value === 'price-desc') {
        result.sort((a, b) => b.price - a.price)
    }

    if (sortOption.value === 'views-desc') {
        result.sort((a, b) => b.viewCount - a.viewCount)
    }

  return result
})

const pageTitle = computed(() => {
  if (keyword.value) {
    return `“${keyword.value}”的搜索结果`
  }

  if (categoryId.value) {
    return '分类商品'
  }

  return '全部闲置'
})

function resetFilters(): void {
  selectedStatus.value = 'all'
  minPrice.value = null
  maxPrice.value = null
  sortOption.value = 'default'
}
</script>

<template>
  <div class="product-list-page">
    <!-- 加载状态 -->
    <div v-if="loading" class="page-state">
      <el-icon class="loading-icon" :size="34">
        <Loading />
      </el-icon>

      <p>商品加载中...</p>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="errorMessage" class="page-state error-state">
      <h2>加载失败</h2>
      <p>{{ errorMessage }}</p>

      <el-button type="primary" @click="loadProducts">
        重新加载
      </el-button>
    </div>

    <!-- 商品列表主体 -->
    <div v-else class="product-list-content">
      <!-- 页面标题 -->
      <header class="page-header">
        <div>
          <h1>{{ pageTitle }}</h1>

          <p>
            浏览同学们发布的校园闲置，找到适合你的商品
          </p>
        </div>
      </header>

      <div class="product-layout">
        <!-- 左侧筛选栏 -->
        <aside class="filter-panel">
          <div class="filter-header">
            <h2>筛选条件</h2>

            <el-button text type="primary" @click="resetFilters">
              重置
            </el-button>
          </div>

          <!-- 商品状态 -->
          <div class="filter-group">
            <h3>商品状态</h3>

            <el-radio-group
              v-model="selectedStatus"
              class="status-options"
            >
              <el-radio-button value="all">
                全部
              </el-radio-button>

              <el-radio-button :value="0">
                在售
              </el-radio-button>

              <el-radio-button :value="1">
                已售
              </el-radio-button>

              <el-radio-button :value="2">
                已下架
              </el-radio-button>
            </el-radio-group>
          </div>

          <!-- 价格区间 -->
          <div class="filter-group">
            <h3>价格区间</h3>

            <div class="price-range">
              <el-input-number
                v-model="minPrice"
                :min="0"
                :precision="2"
                :controls="false"
                placeholder="最低价"
              />

              <span class="price-separator">至</span>

              <el-input-number
                v-model="maxPrice"
                :min="0"
                :precision="2"
                :controls="false"
                placeholder="最高价"
              />
            </div>

            <p
              v-if="priceRangeError"
              class="filter-error"
            >
              最低价格不能高于最高价格
            </p>
          </div>

          <!-- 当前搜索条件 -->
          <div
            v-if="keyword || categoryId"
            class="filter-group current-condition"
          >
            <h3>当前条件</h3>

            <div class="condition-tags">
              <el-tag
                v-if="keyword"
                type="success"
                effect="plain"
              >
                关键词：{{ keyword }}
              </el-tag>

              <el-tag
                v-if="categoryId"
                type="info"
                effect="plain"
              >
                分类 ID：{{ categoryId }}
              </el-tag>
            </div>
          </div>
        </aside>

        <!-- 右侧商品结果 -->
        <section class="product-results">
          <!-- 排序工具栏 -->
          <div class="result-toolbar">
            <div class="result-count">
              共找到
              <strong>{{ displayedProducts.length }}</strong>
              件商品
            </div>

            <el-select
              v-model="sortOption"
              class="sort-select"
              placeholder="选择排序方式"
            >
              <el-option
                label="默认排序"
                value="default"
              />

              <el-option
                label="价格从低到高"
                value="price-asc"
              />

              <el-option
                label="价格从高到低"
                value="price-desc"
              />

              <el-option
                label="浏览量从高到低"
                value="views-desc"
              />
            </el-select>
          </div>

          <!-- 价格范围错误 -->
          <div
            v-if="priceRangeError"
            class="empty-products"
          >
            <h2>价格范围不正确</h2>
            <p>请修改最低价格或最高价格后重新查看。</p>
          </div>

          <!-- 商品网格 -->
          <div
            v-else-if="displayedProducts.length > 0"
            class="product-grid"
          >
            <ProductListCard
              v-for="product in displayedProducts"
              :key="product.productId"
              :product="product"
            />
          </div>

          <!-- 空数据 -->
          <div v-else class="empty-products">
            <h2>没有找到符合条件的商品</h2>

            <p>
              可以尝试调整价格、状态或搜索关键词。
            </p>

            <el-button type="primary" plain @click="resetFilters">
              重置筛选条件
            </el-button>
          </div>
        </section>
      </div>
    </div>
  </div>
</template>

<style scoped>
.product-list-page {
  min-height: calc(100vh - 72px);
  padding: 36px 20px 64px;
  background: #f5f7f6;
}

.product-list-content {
  width: min(1320px, 100%);
  margin: 0 auto;
}

/* 加载和错误状态 */

.page-state {
  display: flex;
  min-height: 480px;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 14px;
  color: #617069;
  text-align: center;
}

.page-state h2,
.page-state p {
  margin: 0;
}

.loading-icon {
  color: #3e9b79;
  animation: rotate 1.2s linear infinite;
}

.error-state h2 {
  color: #26352f;
  font-size: 22px;
}

@keyframes rotate {
  from {
    transform: rotate(0deg);
  }

  to {
    transform: rotate(360deg);
  }
}

/* 页面标题 */

.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-bottom: 28px;
}

.page-header h1 {
  margin: 0;
  color: #1e2a26;
  font-size: 30px;
  line-height: 1.3;
}

.page-header p {
  margin: 8px 0 0;
  color: #6c7a74;
  font-size: 15px;
}

/* 左右布局 */

.product-layout {
  display: grid;
  grid-template-columns: 260px minmax(0, 1fr);
  gap: 24px;
  align-items: start;
}

/* 筛选栏 */

.filter-panel {
  position: sticky;
  top: 96px;
  padding: 22px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  box-shadow: 0 8px 28px rgb(38 53 47 / 5%);
}

.filter-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-bottom: 16px;
}

.filter-header h2 {
  margin: 0;
  color: #1e2a26;
  font-size: 18px;
}

.filter-group {
  padding: 20px 0;
  border-top: 1px solid #edf1ef;
}

.filter-group h3 {
  margin: 0 0 14px;
  color: #34443d;
  font-size: 15px;
  font-weight: 600;
}

.status-options {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  width: 100%;
  gap: 8px;
}

.status-options :deep(.el-radio-button) {
  width: 100%;
}

.status-options :deep(.el-radio-button__inner) {
  width: 100%;
  border: 1px solid #dfe7e3;
  border-radius: 9px;
  box-shadow: none;
}

.status-options
  :deep(.el-radio-button:first-child .el-radio-button__inner),
.status-options
  :deep(.el-radio-button:last-child .el-radio-button__inner) {
  border-radius: 9px;
}

.status-options
  :deep(.el-radio-button__original-radio:checked + .el-radio-button__inner) {
  color: #ffffff;
  background: #3e9b79;
  border-color: #3e9b79;
  box-shadow: none;
}

/* 价格筛选 */

.price-range {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto minmax(0, 1fr);
  align-items: center;
  gap: 8px;
}

.price-range :deep(.el-input-number) {
  width: 100%;
}

.price-range :deep(.el-input__wrapper) {
  padding-right: 10px;
  padding-left: 10px;
}

.price-range :deep(.el-input__inner) {
  text-align: left;
}

.price-separator {
  color: #87938e;
  font-size: 13px;
}

.filter-error {
  margin: 10px 0 0;
  color: #d84f4f;
  font-size: 13px;
  line-height: 1.5;
}

.condition-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

/* 右侧结果区域 */

.product-results {
  min-width: 0;
}

.result-toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 18px;
  padding: 14px 18px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 14px;
}

.result-count {
  color: #65736d;
  font-size: 14px;
}

.result-count strong {
  margin: 0 4px;
  color: #24735b;
  font-size: 18px;
}

.sort-select {
  width: 190px;
}

/* 商品网格 */

.product-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 18px;
}

/* 空状态 */

.empty-products {
  display: flex;
  min-height: 360px;
  padding: 48px 24px;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  background: #ffffff;
  border: 1px dashed #d5dfda;
  border-radius: 16px;
  color: #74817b;
  text-align: center;
}

.empty-products h2 {
  margin: 0;
  color: #34443d;
  font-size: 20px;
}

.empty-products p {
  margin: 12px 0 20px;
  line-height: 1.7;
}

</style>