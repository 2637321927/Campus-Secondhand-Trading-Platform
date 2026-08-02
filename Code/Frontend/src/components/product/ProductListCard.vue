<script setup lang="ts">
import { useRouter } from 'vue-router'
import type {
  ProductListItemDto,
  ProductStatus
} from '../../types/api/product'

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
    if(status===0){
        return '在售'
    }
    else if(status===1){
        return '已售'
    }
    else if(status===2){
        return '已下架'
    }
    return '未知状态'
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
