<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { getHomeData, getHotProducts } from '../../api/modules/home'
import type { HomeResponseDto } from '../../types/api/home'
import type { CategoryDto } from '../../types/api/category'
import type { ProductCardDto } from '../../types/api/product'
import ProductCard from '../../components/product/ProductCard.vue'

const loading = ref(false)
const errorMessage = ref('')
const homeData = ref<HomeResponseDto | null>(null)
const hotProducts = ref<ProductCardDto[]>([])
const router = useRouter()

async function loadHomeData(): Promise<void> {
  loading.value = true
  errorMessage.value = ''

  try {
    const homeResponse = await getHomeData()
    homeData.value = homeResponse.data

    const hotResponse = await getHotProducts()
    hotProducts.value = hotResponse.data
  } catch (error) {
    errorMessage.value = '首页数据加载失败，请稍后重试'
    console.error('首页数据加载失败：', error)
  } finally {
    loading.value = false
  }
}

function goToProductList(): void {
  router.push('/products')
}

function goToPublish(): void {
  router.push({ name: 'product-publish' })
}

function goToCategory(categoryId: number): void {
  router.push({
    path: '/products',
    query: { categoryId }
  })
}

// 首页分类：只展示大分类，点开显示其小分类，大小分类都可点击
const activeParentCategoryId = ref<number | null>(null)

const rootCategories = computed(() => {
  const all = homeData.value?.categories ?? []
  const roots = all.filter((category) => category.parentId === null)

  return roots.map((root) => ({
    ...root,
    children: all.filter(
      (category) => category.parentId === root.categoryId
    )
  }))
})

function onParentCategoryClick(category: {
  categoryId: number
  children: CategoryDto[]
}): void {
  // 大分类没有小分类时直接进入商品列表
  if (category.children.length === 0) {
    goToCategory(category.categoryId)
    return
  }

  activeParentCategoryId.value =
    activeParentCategoryId.value === category.categoryId
      ? null
      : category.categoryId
}

onMounted(() => {
  loadHomeData()
})
</script>

<template>
  <main class="home-page">
    <div class="home-container">
      <aside class="safety-notice" aria-label="校园交易提醒">
        <span class="safety-notice__icon" aria-hidden="true">✓</span>
        <p>校园闲置交易建议当面验货，确认商品状态后再完成交易。</p>
      </aside>

      <div v-if="loading" class="home-loading" aria-live="polite" aria-busy="true">
        <section class="home-section">
          <div class="section-header">
            <div>
              <el-skeleton-item variant="h3" class="skeleton-title" />
              <el-skeleton-item variant="text" class="skeleton-subtitle" />
            </div>
          </div>
          <div class="category-grid">
            <div v-for="index in 6" :key="index" class="skeleton-category">
              <el-skeleton animated>
                <template #template>
                  <el-skeleton-item variant="circle" class="skeleton-category__icon" />
                  <el-skeleton-item variant="h3" class="skeleton-category__title" />
                  <el-skeleton-item variant="text" class="skeleton-category__text" />
                </template>
              </el-skeleton>
            </div>
          </div>
        </section>

        <section class="home-section">
          <div class="section-header">
            <div>
              <el-skeleton-item variant="h3" class="skeleton-title" />
              <el-skeleton-item variant="text" class="skeleton-subtitle" />
            </div>
          </div>
          <div class="product-grid">
            <div v-for="index in 4" :key="index" class="skeleton-product">
              <el-skeleton animated>
                <template #template>
                  <el-skeleton-item variant="image" class="skeleton-product__image" />
                  <div class="skeleton-product__content">
                    <el-skeleton-item variant="h3" />
                    <el-skeleton-item variant="text" class="skeleton-product__price" />
                    <el-skeleton-item variant="text" />
                  </div>
                </template>
              </el-skeleton>
            </div>
          </div>
        </section>
      </div>

      <section v-else-if="errorMessage" class="home-state home-state--error" aria-live="assertive">
        <span class="home-state__icon" aria-hidden="true">!</span>
        <h2>首页内容加载失败</h2>
        <p>{{ errorMessage }}</p>
        <el-button type="primary" @click="loadHomeData">重新加载</el-button>
      </section>

      <template v-else-if="homeData">
        <section class="home-section" aria-labelledby="category-title">
          <div class="section-header">
            <div>
              <h2 id="category-title">热门分类</h2>
              <p>快速找到你需要的校园闲置</p>
            </div>
          </div>

          <div v-if="rootCategories.length > 0" class="category-area">
            <div class="category-grid">
              <button
                v-for="category in rootCategories"
                :key="category.categoryId"
                class="category-card"
                type="button"
                @click="onParentCategoryClick(category)"
              >
                <span class="category-card__content">
                  <strong>{{ category.categoryName }}</strong>
                  <small v-if="category.children.length">{{ category.children.length }} 个子分类</small>
                  <small v-else>查看该分类商品</small>
                </span>
                <span class="category-card__arrow" aria-hidden="true">
                  {{ activeParentCategoryId === category.categoryId ? '−' : '+' }}
                </span>
              </button>
            </div>

            <div
              v-for="category in rootCategories"
              :key="`children-${category.categoryId}`"
            >
              <div
                v-if="activeParentCategoryId === category.categoryId && category.children.length > 0"
                class="category-children"
              >
                <button
                  class="category-chip category-chip--all"
                  type="button"
                  @click="goToCategory(category.categoryId)"
                >
                  {{ category.categoryName }} · 全部
                </button>
                <button
                  v-for="child in category.children"
                  :key="child.categoryId"
                  class="category-chip"
                  type="button"
                  @click="goToCategory(child.categoryId)"
                >
                  {{ child.categoryName }}
                </button>
              </div>
            </div>
          </div>

          <div v-else class="empty-state empty-state--compact">
            <span class="empty-state__icon" aria-hidden="true">□</span>
            <h3>暂时还没有分类</h3>
            <p>分类数据上线后会显示在这里</p>
          </div>
        </section>

        <section class="home-section" aria-labelledby="latest-title">
          <div class="section-header">
            <div>
              <h2 id="latest-title">最新闲置</h2>
              <p>看看同学们最近发布了什么</p>
            </div>
            <el-button text class="section-more" @click="goToProductList">查看更多 <span aria-hidden="true">→</span></el-button>
          </div>

          <div v-if="homeData.recommendedProducts.length > 0" class="product-grid">
            <ProductCard
              v-for="product in homeData.recommendedProducts"
              :key="product.productId"
              :product="product"
            />
          </div>

          <div v-else class="empty-state">
            <span class="empty-state__icon empty-state__icon--warm" aria-hidden="true">＋</span>
            <h3>暂时还没有在售商品</h3>
            <p>成为第一个发布闲置物品的同学吧</p>
            <el-button type="primary" plain @click="goToPublish">发布闲置</el-button>
          </div>
        </section>

        <section class="home-section" aria-labelledby="popular-title">
          <div class="section-header">
            <div>
              <h2 id="popular-title">热门商品</h2>
              <p>大家最近都在关注这些闲置</p>
            </div>
            <el-button text class="section-more" @click="goToProductList">查看更多 <span aria-hidden="true">→</span></el-button>
          </div>

          <div v-if="hotProducts.length > 0" class="product-grid">
            <ProductCard v-for="product in hotProducts" :key="product.productId" :product="product" />
          </div>

          <div v-else class="empty-state">
            <span class="empty-state__icon" aria-hidden="true">↗</span>
            <h3>暂时还没有热门商品</h3>
            <p>商品产生浏览记录后会显示在这里</p>
          </div>
        </section>
      </template>

    </div>
  </main>
</template>

<style scoped>
.home-page { min-height: 100vh; padding: 20px 0 72px; color: #1e2a26; background: #f5f7f6; }
.home-container { width: 100%; max-width: 1320px; margin: 0 auto; padding: 0 24px; }
.safety-notice { display: flex; min-height: 44px; align-items: center; gap: 10px; padding: 10px 16px; color: #355f51; background: #eaf4f0; border: 1px solid #d7e9e2; border-radius: 13px; font-size: 14px; line-height: 1.5; }
.safety-notice__icon { display: inline-grid; width: 22px; height: 22px; flex: 0 0 22px; place-items: center; color: #fff; background: #3e9b79; border-radius: 50%; font-size: 13px; font-weight: 700; }
.home-section { margin-top: 52px; }
.safety-notice + .home-section,
.safety-notice + .home-state,
.safety-notice + .home-loading .home-section:first-child { margin-top: 36px; }
.section-header { display: flex; align-items: flex-end; justify-content: space-between; gap: 20px; margin-bottom: 20px; }
.section-header h2 { margin: 0; color: #1e2a26; font-size: 25px; font-weight: 700; letter-spacing: -0.4px; line-height: 1.3; }
.section-header p { margin-top: 7px; color: #6c7a74; font-size: 14px; line-height: 1.6; }
.section-more { flex-shrink: 0; color: #24735b; font-weight: 600; }
.section-more span { margin-left: 3px; transition: transform 0.2s ease; }
.section-more:hover span { transform: translateX(3px); }
.category-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 16px; }
.category-card { display: flex; min-height: 112px; align-items: center; gap: 16px; padding: 18px 20px; color: #1e2a26; text-align: left; background: #fff; border: 1px solid #e3e9e6; border-radius: 16px; cursor: pointer; transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease; }
.category-card:hover { border-color: #9fcab9; box-shadow: 0 10px 24px rgba(30, 72, 57, 0.07); transform: translateY(-3px); }
.category-card:focus-visible { outline: 3px solid rgba(36, 115, 91, 0.23); outline-offset: 3px; }
.category-card__content { display: flex; min-width: 0; flex: 1; flex-direction: column; gap: 7px; }
.category-card__content strong { overflow: hidden; font-size: 16px; text-overflow: ellipsis; white-space: nowrap; }
.category-card__content small { color: #6c7a74; font-size: 12px; }
.category-card__arrow { color: #9aa9a3; font-size: 17px; }

.category-area { display: flex; flex-direction: column; gap: 14px; }

.category-children { display: flex; flex-wrap: wrap; gap: 10px; padding: 14px 16px; background: #fbfdfc; border: 1px solid #e3e9e6; border-radius: 14px; }

.category-chip { padding: 6px 14px; color: #34443d; font-size: 13px; background: #fff; border: 1px solid #d5dfda; border-radius: 999px; cursor: pointer; transition: border-color 0.2s ease, color 0.2s ease, background 0.2s ease; }

.category-chip:hover { color: #24735b; border-color: #3e9b79; background: #eef7f3; }

.category-chip--all { color: #24735b; font-weight: 600; border-color: #3e9b79; }
.product-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 20px; }
.empty-state, .home-state { display: flex; min-height: 220px; align-items: center; justify-content: center; padding: 34px 20px; text-align: center; flex-direction: column; background: #fff; border: 1px solid #e3e9e6; border-radius: 18px; }
.empty-state--compact { min-height: 176px; }
.empty-state__icon, .home-state__icon { display: grid; width: 52px; height: 52px; margin-bottom: 14px; place-items: center; color: #24735b; background: #eaf4f0; border-radius: 16px; font-size: 24px; font-weight: 700; }
.empty-state__icon--warm { color: #a36121; background: #fff3e6; }
.empty-state h3, .home-state h2 { margin: 0; color: #1e2a26; font-size: 18px; font-weight: 700; }
.empty-state p, .home-state p { margin: 8px 0 0; color: #6c7a74; font-size: 14px; line-height: 1.6; }
.empty-state :deep(.el-button), .home-state :deep(.el-button) { margin-top: 18px; border-radius: 10px; }
.home-state--error { min-height: 300px; margin-top: 48px; background: #fffdfc; border-color: #f0d7d4; }
.home-state--error .home-state__icon { color: #d9544d; background: #faecea; }
.skeleton-title { width: 130px; height: 26px; }
.skeleton-subtitle { display: block; width: 220px; margin-top: 10px; }
.skeleton-category, .skeleton-product { overflow: hidden; background: #fff; border: 1px solid #e3e9e6; border-radius: 16px; }
.skeleton-category { min-height: 112px; padding: 18px; }
.skeleton-category :deep(.el-skeleton__content) { display: grid; align-items: center; grid-template-columns: 48px 1fr; column-gap: 13px; }
.skeleton-category__icon { width: 48px; height: 48px; grid-row: span 2; }
.skeleton-category__title { width: 70%; }
.skeleton-category__text { width: 90%; }
.skeleton-product__image { width: 100%; height: auto; aspect-ratio: 4 / 3; border-radius: 0; }
.skeleton-product__content { display: grid; gap: 12px; padding: 16px; }
.skeleton-product__price { width: 42%; height: 22px; }

@media (max-width: 1100px) {
  .product-grid { grid-template-columns: repeat(3, minmax(0, 1fr)); }
}

@media (max-width: 820px) {
  .home-page { padding-top: 16px; }
  .home-container { padding: 0 16px; }
  .home-section { margin-top: 44px; }
  .product-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
}

@media (max-width: 520px) {
  .safety-notice { align-items: flex-start; }
  .section-header { align-items: flex-start; flex-wrap: wrap; margin-bottom: 16px; }
  .section-header h2 { font-size: 22px; }
  .category-grid, .product-grid { grid-template-columns: 1fr; }
  .empty-state { min-height: 200px; }
}

@media (prefers-reduced-motion: reduce) {
  .category-card, .section-more span { transition: none; }
}
</style>
