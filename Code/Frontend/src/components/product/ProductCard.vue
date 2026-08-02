<script setup lang="ts">
import type { ProductCardDto } from '../../types/api/product';
import { formatDate } from '../../utils/format';
import { useRouter } from 'vue-router'

const props=defineProps<{
    product:ProductCardDto
    imageUrl?: string
}>()

const router=useRouter()

function goToDetail():void{
    router.push(`/products/${props.product.productId}`)
}

</script>

<template>
    <article 
    class="product-card"
    @click="goToDetail">
        <div class="product-cover">
            <el-image
            v-if="imageUrl"
            :src="imageUrl"
            fit="cover"/>
            <div v-else class="image-placeholder">
                暂无图片
            </div>
        </div>
        <h3 class="product-title">
        {{ product.name }}
        </h3>
        <p class="product-price">
        ¥{{ product.price.toFixed(2) }}
        </p>

        <div class="product-meta">
            <span>{{ product.sellerName }}</span>
            <span>{{ formatDate(product.releaseDate) }}</span>
            <span>{{ product.viewCount }} 次浏览</span>
        </div>
    </article>
</template>

<style scoped>
.product-card {
  overflow: hidden;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.product-card:hover {
  transform: translateY(-4px);
}

.product-cover {
  width: 100%;
  aspect-ratio: 4 / 3;
}

.product-title {
  display: -webkit-box;
  overflow: hidden;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 2;
}

.product-meta {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 6px 12px;
  color: #6c7a74;
  font-size: 13px;
  text-align: center;
}

.product-cover :deep(.el-image) {
  width: 100%;
  height: 100%;
}

.image-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  background: #f5f7f6;
  color: #6c7a74;
}
</style>
