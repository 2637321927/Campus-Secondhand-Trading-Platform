<template>
  <div class="product-review">
    <!-- 统计卡片 -->
    <el-row :gutter="20" class="stats-row">
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.pendingReviewCount || 0 }}</div>
            <div class="stat-label">待审核</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.totalProducts || 0 }}</div>
            <div class="stat-label">商品总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.rejectedCount || 0 }}</div>
            <div class="stat-label">已驳回</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.availableCount || 0 }}</div>
            <div class="stat-label">在售</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 商品列表 -->
    <el-card class="table-card">
      <template #header>
        <div class="card-header">
          <span>待审核商品</span>
          <el-button @click="loadData" :loading="loading">刷新</el-button>
        </div>
      </template>

      <el-table :data="productList" v-loading="loading" border>
        <el-table-column prop="productId" label="ID" width="80" />
        <el-table-column label="商品信息" min-width="200">
          <template #default="{ row }">
            <div class="product-info">
              <el-image
                :src="getProductImageUrl(row.coverImageFileId)"
                class="product-cover"
                fit="cover"
              >
                <template #error>
                  <div class="image-placeholder">暂无</div>
                </template>
              </el-image>
              <div>
                <div class="product-name">{{ row.name }}</div>
                <div class="product-price">¥{{ Number(row.price).toFixed(2) }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="sellerName" label="卖家" width="120" />
        <el-table-column prop="categoryName" label="分类" width="100" />
        <el-table-column label="操作" width="280" fixed="right">
          <template #default="{ row }">
            <el-button type="success" size="small" @click="handleApprove(row)">
              通过
            </el-button>
            <el-button type="danger" size="small" @click="handleReject(row)">
              驳回
            </el-button>
            <el-button type="primary" size="small" @click="viewDetail(row)">
              详情
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-model:current-page="page"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50, 100]"
        layout="total, sizes, prev, pager, next, jumper"
        @size-change="loadData"
        @current-change="loadData"
        class="pagination"
      />
    </el-card>

    <!-- 驳回对话框 -->
    <el-dialog v-model="rejectDialogVisible" title="驳回原因" width="500px">
      <el-form>
        <el-form-item label="驳回原因">
          <el-input
            v-model="rejectReason"
            type="textarea"
            rows="3"
            placeholder="请填写驳回原因"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="rejectDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="confirmReject">确定驳回</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getPendingProducts,
  approveProduct,
  rejectProduct,
  getProductStatistics
} from '../../../api/modules/admin' 

const router = useRouter()
const loading = ref(false)
const productList = ref<any[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const statistics = ref({
  totalProducts: 0,
  pendingReviewCount: 0,
  rejectedCount: 0,
  availableCount: 0
})

const rejectDialogVisible = ref(false)
const rejectReason = ref('')
const currentProduct = ref<any>(null)

// ========== 获取商品图片 URL ==========
// 根据对接说明，使用 /api/files/{fileId} 接口
const getProductImageUrl = (fileId: number | null | undefined) => {
  if (!fileId) return '/default-product.png'
  return `/api/files/${fileId}`
}

// ========== 加载待审核商品列表 ==========
const loadData = async () => {
  loading.value = true
  try {
    // ✅ 使用 getPendingProducts，不是 getAdminProducts
    const res = await getPendingProducts(page.value, pageSize.value)
    console.log('待审核商品响应:', res)
    
    // 直接使用 res.data
    const data = res.data
    productList.value = data.items || []
    total.value = data.totalCount || 0
  } catch (error: any) {
    console.error('加载失败:', error)
    ElMessage.error(error?.message || '加载失败')
  } finally {
    loading.value = false
  }
}

const loadStatistics = async () => {
  try {
    const res = await getProductStatistics()
    console.log('统计响应:', res)
    const data = res?.data || res || {}
    statistics.value = data
  } catch (error) {
    console.error('加载统计失败', error)
  }
}

// ========== 查看详情 - 跳转到管理员专用商品详情页（与商品管理界面一致） ==========
const viewDetail = (row: any) => {
  router.push(`/admin/products/${row.productId}`)
}

const handleApprove = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要通过商品 "${row.name}" 的审核吗？`, '审核通过', {
      type: 'success'
    })
    await approveProduct(row.productId)
    ElMessage.success('审核通过')
    loadData()
    loadStatistics()
  } catch (error) {
    if (error !== 'cancel') {
      console.error('审核通过失败:', error)
      ElMessage.error('操作失败')
    }
  }
}

const handleReject = (row: any) => {
  currentProduct.value = row
  rejectReason.value = ''
  rejectDialogVisible.value = true
}

const confirmReject = async () => {
  if (!rejectReason.value.trim()) {
    ElMessage.warning('请填写驳回原因')
    return
  }
  try {
    await rejectProduct(currentProduct.value.productId, {
      reason: rejectReason.value
    })
    ElMessage.success('已驳回')
    rejectDialogVisible.value = false
    loadData()
    loadStatistics()
  } catch (error) {
    console.error('驳回失败:', error)
    ElMessage.error('操作失败')
  }
}

onMounted(() => {
  loadData()
  loadStatistics()
})
</script>

<style scoped>
.product-review {
  padding: 20px;
}
.stats-row {
  margin-bottom: 20px;
}
.stat-item {
  text-align: center;
}
.stat-number {
  font-size: 28px;
  font-weight: bold;
  color: #24735b;
}
.stat-label {
  color: #666;
  margin-top: 5px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.product-info {
  display: flex;
  align-items: center;
  gap: 12px;
}
.product-cover {
  width: 50px;
  height: 50px;
  border-radius: 8px;
  flex-shrink: 0;
  background: #f5f7f6;
}
.image-placeholder {
  width: 50px;
  height: 50px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f5f7f6;
  color: #ccc;
  font-size: 12px;
  border-radius: 8px;
}
.product-name {
  font-weight: 500;
}
.product-price {
  color: #f56c6c;
  font-weight: bold;
}
.table-card {
  margin-top: 20px;
}
.pagination {
  margin-top: 20px;
  justify-content: flex-end;
}
</style>