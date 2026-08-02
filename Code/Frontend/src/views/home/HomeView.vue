<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { 
    getHomeData, 
    getHotProducts
    } from '../../api/modules/home'
import type { HomeResponseDto } from '../../types/api/home'
import type { ProductCardDto } from '../../types/api/product'
import ProductCard from '../../components/product/ProductCard.vue'
import {useRouter} from 'vue-router'
import { useProductImages } from '../../composables/useProductImages'

const loading = ref(false)
const errorMessage = ref('')
const homeData = ref<HomeResponseDto | null>(null)
const hotProducts=ref<ProductCardDto[]>([])
const {
  loadProductImages,
  getProductImageUrl
} = useProductImages()

async function loadHomeData():Promise<void> {
    loading.value=true
    errorMessage.value=''
    try{
        const homeResponse=await getHomeData()
        homeData.value=homeResponse.data

        const hotResponse=await getHotProducts()
        hotProducts.value=hotResponse.data

        const products = [
          ...(homeData.value?.recommendedProducts ?? []),
          ...hotProducts.value
        ]

        await loadProductImages(
          products.map((product) => product.coverImageFileId)
        ).catch((error) => {
          console.error('商品图片加载失败：', error)
        })
    }
    catch(error){
        errorMessage.value = '首页数据加载失败，请稍后重试'
        console.error('首页数据加载失败：', error)
    }
    finally{
        loading.value=false
    }
}

const router=useRouter()

function goToProductList():void{
    router.push('/products')
}

function goToCategory(categoryId: number): void {
  router.push({
    path: '/products',
    query: {
      categoryId
    }
  })
}

onMounted(() => {
  loadHomeData()
})
</script>

<template>

  <main class="home-page">
    <!-- 加载状态 -->
    <div v-if="loading" class="home-state">
      <p>首页数据加载中...</p>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="errorMessage" class="home-state">
      <p>{{ errorMessage }}</p>

      <el-button type="primary" @click="loadHomeData">
        重新加载
      </el-button>
    </div>

    <!-- 成功状态 -->
    <div v-else-if="homeData">
      <!-- 热门分类 -->
      <section class="home-section">
        <div class="section-header">
          <div>
            <h2>热门分类</h2>
            <p>快速找到你需要的校园闲置</p>
          </div>
        </div>

        <div
          v-if="homeData.categories.length > 0"
          class="category-grid"
        >
          <button
            v-for="category in homeData.categories"
            :key="category.categoryId"
            class="category-card"
            type="button"
            @click="goToCategory(category.categoryId)"
          >
            <strong>{{ category.categoryName }}</strong>

            <span v-if="category.children?.length">
              {{ category.children.length }} 个子分类
            </span>

            <span v-else>
              查看该分类商品
            </span>
          </button>
        </div>

        <div v-else class="empty-categories">
          暂时没有可用分类
        </div>
      </section>

      <!-- 最新闲置 -->
      <section class="home-section">
        <div class="section-header">
          <div>
            <h2>最新闲置</h2>
            <p>看看同学们最近发布了什么</p>
          </div>

          <el-button text @click="goToProductList">
            查看更多
          </el-button>
        </div>

        <div
          v-if="homeData.recommendedProducts.length > 0"
          class="product-grid"
        >
          <ProductCard
            v-for="product in homeData.recommendedProducts"
            :key="product.productId"
            :product="product"
            :image-url="getProductImageUrl(product.coverImageFileId)"
          />
        </div>

        <div v-else class="empty-products">
          <p>暂时还没有在售商品</p>
          <span>成为第一个发布闲置物品的同学吧</span>
        </div>
      </section>

      <!-- 热门商品 -->
      <section class="home-section">
        <div class="section-header">
          <div>
            <h2>热门商品</h2>
            <p>大家最近都在关注这些闲置</p>
          </div>

          <el-button text @click="goToProductList">
            查看更多
          </el-button>
        </div>

        <div
          v-if="hotProducts.length > 0"
          class="product-grid"
        >
          <ProductCard
            v-for="product in hotProducts"
            :key="product.productId"
            :product="product"
            :image-url="getProductImageUrl(product.coverImageFileId)"
          />
        </div>

        <div v-else class="empty-products">
          <p>暂时还没有热门商品</p>
          <span>商品产生浏览记录后会显示在这里</span>
        </div>
      </section>
    </div>
  </main>
</template>

<style scoped>
.product-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 20px;
}
</style>
