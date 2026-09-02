// Review the products pending listing

<template>
  <div class="product-review">
    <!-- 统计卡片 -->
    <el-row :gutter="20" class="stats-row">
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.pendingReviewCount }}</div>
            <div class="stat-label">待审核</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.totalProducts }}</div>
            <div class="stat-label">商品总数</div>
          </div>
        </el-card>
      </el-col>
      <!-- 更多统计... -->
    </el-row>

    <!-- 商品列表 -->
    <el-card class="table-card">
      <template #header>
        <div class="card-header">
          <span>待审核商品</span>
          <el-button @click="loadData" :loading="loading">刷新</el-button>
        </div>
      </template>

      <el-table :data="productList" v-loading="loading">
        <el-table-column prop="productId" label="ID" width="80" />
        <el-table-column prop="name" label="商品名称" min-width="150" />
        <el-table-column prop="price" label="价格" width="100">
          <template #default="{ row }">¥{{ row.price }}</template>
        </el-table-column>
        <el-table-column prop="sellerName" label="卖家" width="120" />
        <el-table-column prop="categoryName" label="分类" width="100" />
        <el-table-column label="操作" width="250" fixed="right">
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
        @current-change="loadData"
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
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getPendingProducts,
  approveProduct,
  rejectProduct,
  getProductStatistics
} from '../../../api/modules/admin' 

const loading = ref(false)
const productList = ref([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const statistics = ref({
  totalProducts: 0,
  pendingReviewCount: 0,
  // ...
})

const rejectDialogVisible = ref(false)
const rejectReason = ref('')
const currentProduct = ref<any>(null)

const loadData = async () => {
  loading.value = true
  try {
    const res = await getPendingProducts(page.value, pageSize.value)
    productList.value = res.items
    total.value = res.totalCount
  } catch (error) {
    ElMessage.error('加载失败')
  } finally {
    loading.value = false
  }
}

const loadStatistics = async () => {
  try {
    statistics.value = await getProductStatistics()
  } catch (error) {
    console.error('加载统计失败', error)
  }
}

const handleApprove = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要通过商品 "${row.name}" 的审核吗？`, '提示')
    await approveProduct(row.productId)
    ElMessage.success('审核通过')
    loadData()
    loadStatistics()
  } catch (error) {
    if (error !== 'cancel') {
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
    ElMessage.error('操作失败')
  }
}

const viewDetail = (row: any) => {
  // 跳转到商品详情页
  // router.push(`/admin/products/${row.productId}`)
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
</style>