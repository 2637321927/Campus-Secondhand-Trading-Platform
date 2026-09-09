import type { ProductStatus } from '../types/api/product'

export const PRODUCT_STATUS_TEXT: Record<ProductStatus, string> = {
  0: '在售',
  1: '已售',
  2: '已下架',
  3: '待审核',
  4: '审核驳回',
  5: '交易中'
}

export const PRODUCT_STATUS_TAG_TYPE: Record<
  ProductStatus,
  'success' | 'info' | 'danger' | 'warning' | 'primary'
> = {
  0: 'success',
  1: 'info',
  2: 'danger',
  3: 'warning',
  4: 'danger',
  5: 'primary'
}

export function getProductStatusText(status: ProductStatus): string {
  return PRODUCT_STATUS_TEXT[status] ?? '未知状态'
}

export function getProductStatusTagType(
  status: ProductStatus
): 'success' | 'info' | 'danger' | 'warning' | 'primary' {
  return PRODUCT_STATUS_TAG_TYPE[status] ?? 'info'
}
