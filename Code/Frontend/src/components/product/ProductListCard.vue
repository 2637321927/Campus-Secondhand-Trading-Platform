<script setup lang="ts">
import { useRouter } from 'vue-router'
import type {
  ProductListItemDto,
  ProductStatus
} from '../../types/api/product'
import { getProductStatusText } from '../../utils/productStatus'

const props = defineProps<{
  product: ProductListItemDto
  imageUrl?: string
}>()

const router=useRouter()

function goToDetail():void{
    router.push(
        `/products/${props.product.productId}`
    )
}

function getStatusText(status:ProductStatus):string{
    return getProductStatusText(status)
}
</script>

<template>
  <article
    class="product-list-card"
    role="button"
    tabindex="0"
    @click="goToDetail"
  >
    <div class="product-cover">
      <el-image
        v-if="imageUrl"
        :src="imageUrl"
        fit="cover"
      />

      <div v-else class="image-placeholder">
        暂无图片
      </div>
    </div>

    <div class="product-content">
      <div class="product-header">
        <h3 class="product-title">
          {{ product.name }}
        </h3>

        <span
          v-if="product.status !== undefined"
          class="product-status"
        >
          {{ getStatusText(product.status) }}
        </span>
      </div>

      <p class="product-price">
        ¥{{ product.price.toFixed(2) }}
      </p>

      <p v-if="product.categoryName" class="product-category">
        {{ product.categoryName }}
      </p>

      <p v-if="product.info" class="product-info">
        {{ product.info }}
      </p>

      <p class="product-views">
        {{ product.viewCount }} 次浏览
      </p>
    </div>
  </article>
</template>

<style scoped>
.product-list-card {
  display: flex;
  height: 100%;
  min-width: 0;
  overflow: hidden;
  flex-direction: column;
  color: #1e2a26;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  cursor: pointer;
  transition: transform 0.2s ease, border-color 0.2s ease, box-shadow 0.2s ease;
}

.product-list-card:hover {
  border-color: #abd0c1;
  box-shadow: 0 10px 24px rgb(31 77 60 / 8%);
  transform: translateY(-3px);
}

.product-list-card:focus-visible {
  border-color: #3e9b79;
  outline: 3px solid rgb(36 115 91 / 23%);
  outline-offset: 3px;
}

.product-cover {
  width: 100%;
  overflow: hidden;
  flex: 0 0 auto;
  aspect-ratio: 4 / 3;
  background: #edf2f0;
}

.product-cover :deep(.el-image) {
  width: 100%;
  height: 100%;
}

.product-cover :deep(.el-image__inner) {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.25s ease;
}

.product-list-card:hover .product-cover :deep(.el-image__inner) {
  transform: scale(1.025);
}

.image-placeholder {
  display: grid;
  width: 100%;
  height: 100%;
  place-items: center;
  color: #7a8882;
  background: #edf2f0;
  font-size: 13px;
}

.product-content {
  display: flex;
  min-height: 0;
  flex: 1;
  flex-direction: column;
  padding: 16px;
}

.product-header {
  display: flex;
  min-height: 46px;
  align-items: flex-start;
  gap: 10px;
}

.product-title {
  display: -webkit-box;
  min-width: 0;
  margin: 0;
  overflow: hidden;
  flex: 1;
  color: #1e2a26;
  -webkit-box-orient: vertical;
  font-size: 16px;
  font-weight: 650;
  -webkit-line-clamp: 2;
  line-height: 1.42;
}

.product-status {
  flex: 0 0 auto;
  padding: 3px 8px;
  color: #24735b;
  background: #eaf4f0;
  border-radius: 999px;
  font-size: 12px;
  line-height: 1.5;
}

.product-price {
  margin: 10px 0 12px;
  color: #24735b;
  font-size: 21px;
  font-weight: 750;
  letter-spacing: -0.4px;
  line-height: 1.2;
}

.product-category {
  margin: 0 0 8px;
  overflow: hidden;
  color: #50635b;
  font-size: 13px;
  line-height: 1.5;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-info {
  display: -webkit-box;
  min-height: 42px;
  margin: 0;
  overflow: hidden;
  color: #6c7a74;
  -webkit-box-orient: vertical;
  font-size: 13px;
  -webkit-line-clamp: 2;
  line-height: 1.6;
}

.product-views {
  margin: auto 0 0;
  padding-top: 12px;
  color: #84918c;
  border-top: 1px solid #edf1ef;
  font-size: 12px;
  line-height: 1.5;
}

@media (max-width: 600px) {
  .product-content {
    padding: 14px;
  }

  .product-price {
    font-size: 20px;
  }
}

@media (prefers-reduced-motion: reduce) {
  .product-list-card,
  .product-cover :deep(.el-image__inner) {
    transition: none;
  }
}
</style>
