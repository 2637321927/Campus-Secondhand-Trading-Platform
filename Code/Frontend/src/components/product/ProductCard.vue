<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import type { ProductCardDto } from '../../types/api/product'
import { resolveImageUrl } from '../../utils/image'
import { formatDate } from '../../utils/format'

const props = defineProps<{
  product: ProductCardDto
}>()

const coverUrl = computed(() => {
  if (props.product.coverImageUrl) {
    return resolveImageUrl(props.product.coverImageUrl)
  }

  return undefined
})

const router = useRouter()

function goToDetail(): void {
  router.push(`/products/${props.product.productId}`)
}
</script>

<template>
  <article
    class="product-card"
    role="link"
    tabindex="0"
    :aria-label="`查看商品：${product.name}`"
    @click="goToDetail"
    @keydown.enter="goToDetail"
    @keydown.space.prevent="goToDetail"
  >
    <div class="product-cover">
      <el-image
        v-if="coverUrl"
        :src="coverUrl"
        :alt="`${product.name} 商品图片`"
        fit="cover"
        lazy
      />
      <div v-else class="image-placeholder">
        <span class="image-placeholder__icon" aria-hidden="true">▧</span>
        <span>暂无图片</span>
      </div>
    </div>

    <div class="product-card__content">
      <h3 class="product-title">{{ product.name }}</h3>
      <p class="product-price">
        <span class="product-price__symbol">¥</span>{{ product.price.toFixed(2) }}
      </p>

      <div class="product-seller">
        <span class="product-seller__avatar" aria-hidden="true">
          {{ product.sellerName.slice(0, 1) || '同' }}
        </span>
        <span class="product-seller__name">{{ product.sellerName }}</span>
      </div>

      <div class="product-meta">
        <span>{{ formatDate(product.releaseDate) }}</span>
        <span class="product-meta__divider" aria-hidden="true"></span>
        <span>{{ product.viewCount }} 次浏览</span>
      </div>
    </div>
  </article>
</template>

<style scoped>
.product-card {
  min-width: 0;
  overflow: hidden;
  color: #1e2a26;
  background: #fff;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  cursor: pointer;
  transition: transform 0.2s ease, box-shadow 0.2s ease, border-color 0.2s ease;
}

.product-card:hover {
  border-color: #abd0c1;
  box-shadow: 0 12px 26px rgba(31, 77, 60, 0.09);
  transform: translateY(-4px);
}

.product-card:focus-visible {
  outline: 3px solid rgba(36, 115, 91, 0.23);
  outline-offset: 3px;
  border-color: #3e9b79;
}

.product-cover {
  width: 100%;
  overflow: hidden;
  aspect-ratio: 4 / 3;
  background: #edf2f0;
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

.product-cover :deep(.el-image__inner) {
  transition: transform 0.25s ease;
}

.product-card:hover .product-cover :deep(.el-image__inner) {
  transform: scale(1.025);
}

.image-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  gap: 8px;
  color: #7a8882;
  background: #edf2f0;
  font-size: 13px;
}

.image-placeholder__icon {
  font-size: 20px;
}

.product-card__content {
  padding: 15px 16px 16px;
}

.product-title {
  display: -webkit-box;
  min-height: 44px;
  margin: 0;
  overflow: hidden;
  color: #1e2a26;
  -webkit-box-orient: vertical;
  font-size: 16px;
  font-weight: 650;
  -webkit-line-clamp: 2;
  line-height: 1.42;
}

.product-price {
  margin: 11px 0 14px;
  color: #24735b;
  font-size: 21px;
  font-weight: 750;
  letter-spacing: -0.4px;
  line-height: 1.2;
}

.product-price__symbol {
  margin-right: 2px;
  font-size: 14px;
}

.product-seller {
  display: flex;
  min-width: 0;
  align-items: center;
  gap: 8px;
}

.product-seller__avatar {
  display: grid;
  width: 25px;
  height: 25px;
  flex: 0 0 25px;
  place-items: center;
  color: #24735b;
  background: #eaf4f0;
  border-radius: 50%;
  font-size: 11px;
  font-weight: 700;
}

.product-seller__name {
  overflow: hidden;
  color: #50635b;
  font-size: 13px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.product-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 11px;
  color: #84918c;
  font-size: 12px;
  line-height: 1.5;
}

.product-meta__divider {
  width: 3px;
  height: 3px;
  background: #bdc8c3;
  border-radius: 50%;
}

@media (max-width: 600px) {
  .product-card__content { padding: 13px 14px 14px; }
  .product-price { font-size: 20px; }
}

@media (prefers-reduced-motion: reduce) {
  .product-card,
  .product-cover :deep(.el-image__inner) { transition: none; }
}
</style>
