<script setup lang="ts">
import { ref } from 'vue'
import {
  ElMessage,
  ElMessageBox
} from 'element-plus'
import {
  deleteProduct,
  getProductDetail,
  updateProduct
} from '../../api/modules/product'
import type {
  ProductStatus
} from '../../types/api/product'

const props = defineProps<{
  productId: number
  status: ProductStatus
  disabled?: boolean
}>()

const emit = defineEmits<{
  changed: []
  deleted: []
}>()

type ActionName =
  | 'delete'
  | 'sold'
  | 'relist'
  | 'offline'

const actionLoading = ref<ActionName | null>(null)

function isActionLoading(action: ActionName): boolean {
  return actionLoading.value === action
}

async function updateStatus(status: ProductStatus): Promise<void> {
  const product = (await getProductDetail(props.productId)).data

  await updateProduct(props.productId, {
    name: product.name,
    price: product.price,
    info: product.info ?? '',
    categoryId: product.categoryId,
    status,
    newImages: [],
    toRemoveImageIds: [],
    shippingType: product.shippingType,
    shippingFee: product.shippingFee ?? null,
    allowPickup: product.allowPickup
  })
}

async function confirmAction(
  action: ActionName,
  message: string,
  title: string
): Promise<boolean> {
  if (actionLoading.value !== null) {
    return false
  }

  actionLoading.value = action

  try {
    await ElMessageBox.confirm(
      message,
      title,
      {
        confirmButtonText: '确认',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )

    return true
  } catch {
    actionLoading.value = null
    return false
  }
}

async function handleDelete(): Promise<void> {
  const confirmed = await confirmAction(
    'delete',
    '删除后无法恢复，确定删除这件商品吗？',
    '删除商品'
  )

  if (!confirmed) {
    return
  }

  try {
    await deleteProduct(props.productId)
    ElMessage.success('商品已删除')
    emit('deleted')
  } catch (error) {
    console.error('删除商品失败：', error)
    ElMessage.error('删除商品失败，请稍后重试')
  } finally {
    actionLoading.value = null
  }
}

async function handleMarkSold(): Promise<void> {
  const confirmed = await confirmAction(
    'sold',
    '标记为已售后，其他用户将无法继续购买。确定继续吗？',
    '标记已售'
  )

  if (!confirmed) {
    return
  }

  try {
    await updateStatus(1)
    ElMessage.success('商品已标记为已售')
    emit('changed')
  } catch (error) {
    console.error('标记已售失败：', error)
    ElMessage.error('标记已售失败，请稍后重试')
  } finally {
    actionLoading.value = null
  }
}

async function handleRelist(): Promise<void> {
  const confirmed = await confirmAction(
    'relist',
    '确定将这件商品重新上架吗？',
    '重新上架'
  )

  if (!confirmed) {
    return
  }

  try {
    await updateStatus(0)
    ElMessage.success('商品已重新上架')
    emit('changed')
  } catch (error) {
    console.error('重新上架失败：', error)
    ElMessage.error('重新上架失败，请稍后重试')
  } finally {
    actionLoading.value = null
  }
}

async function changeStatus(
  action: 'offline',
  status: ProductStatus,
  message: string,
  title: string,
  successMessage: string
): Promise<void> {
  const confirmed = await confirmAction(
    action,
    message,
    title
  )

  if (!confirmed) {
    return
  }

  try {
    await updateStatus(status)

    ElMessage.success(successMessage)
    emit('changed')
  } catch (error) {
    console.error(`${title}失败：`, error)
    ElMessage.error(`${title}失败，请稍后重试`)
  } finally {
    actionLoading.value = null
  }
}

async function handleOffline(): Promise<void> {
  await changeStatus(
    'offline',
    2,
    '下架后商品将不再对买家展示。确定继续吗？',
    '下架商品',
    '商品已下架'
  )
}

</script>

<template>
  <div class="seller-product-actions">
    <el-button
      v-if="status === 0"
      type="warning"
      plain
      :loading="isActionLoading('sold')"
      :disabled="disabled || actionLoading !== null"
      @click="handleMarkSold"
    >
      标记已售
    </el-button>

    <el-button
      v-if="status === 0"
      plain
      :loading="isActionLoading('offline')"
      :disabled="disabled || actionLoading !== null"
      @click="handleOffline"
    >
      下架
    </el-button>

    <el-button
      v-if="status === 1 || status === 2"
      type="primary"
      plain
      :loading="isActionLoading('relist')"
      :disabled="disabled || actionLoading !== null"
      @click="handleRelist"
    >
      重新上架
    </el-button>

    <el-button
      type="danger"
      plain
      :loading="isActionLoading('delete')"
      :disabled="disabled || actionLoading !== null"
      @click="handleDelete"
    >
      删除
    </el-button>
  </div>
</template>

<style scoped>
.seller-product-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.seller-product-actions :deep(.el-button + .el-button) {
  margin-left: 0;
}
</style>
