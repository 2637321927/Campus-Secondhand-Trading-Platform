<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { resolveFileUrl } from '../../utils/image'
import type { ProductDto } from '../../types/api/product';
import type { ProductStatus } from '../../types/api/product';

const props = defineProps<{
  product: ProductDto
}>()

const coverUrl=computed(()=>{
    const images = [...(props.product.images ?? [])]

    images.sort((a,b)=>{
        return a.imgIndex-b.imgIndex
    })

    const firstImage=images[0]

    if(!firstImage){
      return ''
    }

    if(images[0].imgFileId)
        return resolveFileUrl(images[0].imgFileId)
    else
        return resolveFileUrl(null)
})

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
    return '已下架'
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
        v-if="coverUrl"
        :src="coverUrl"
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

        <span class="product-status">
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