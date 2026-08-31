// 商品管理
<template>
  <div class="product-manage">
    <!-- 统计卡片 -->
    <el-row :gutter="20" class="stats-row">
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.totalProducts }}</div>
            <div class="stat-label">商品总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.availableCount }}</div>
            <div class="stat-label">在售</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.pendingReviewCount }}</div>
            <div class="stat-label">待审核</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.rejectedCount }}</div>
            <div class="stat-label">已驳回</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.removedCount }}</div>
            <div class="stat-label">已下架</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.soldCount }}</div>
            <div class="stat-label">已售</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 筛选栏 -->
    <el-card class="filter-card">
      <el-form :inline="true" :model="queryParams" class="filter-form">
        <el-form-item label="关键词">
          <el-input
            v-model="queryParams.keyword"
            placeholder="商品名/卖家昵称"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="queryParams.status" placeholder="全部状态" clearable>
            <el-option label="在售" :value="0" />
            <el-option label="已售" :value="1" />
            <el-option label="已下架" :value="2" />
            <el-option label="待审核" :value="3" />
            <el-option label="已驳回" :value="4" />
          </el-select>
        </el-form-item>
        <el-form-item label="分类">
          <el-select v-model="queryParams.categoryId" placeholder="全部分类" clearable>
            <el-option
              v-for="cat in categories"
              :key="cat.categoryId"
              :label="cat.categoryName"
              :value="cat.categoryId"
            />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="resetSearch">重置</el-button>
          <el-button @click="loadData" :loading="loading">刷新</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 商品列表 -->
    <el-card class="table-card">
      <el-table :data="productList" v-loading="loading" border>
        <el-table-column prop="productId" label="ID" width="70" />
        <el-table-column label="商品信息" min-width="200">
          <template #default="{ row }">
            <div class="product-info">
              <el-image
                :src="row.coverImage || '/default-image.png'"
                class="product-cover"
                fit="cover"
              />
              <div>
                <div class="product-name">{{ row.name }}</div>
                <div class="product-price">¥{{ row.price }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="sellerName" label="卖家" width="120" />
        <el-table-column prop="categoryName" label="分类" width="100" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ getStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="数据" width="180">
          <template #default="{ row }">
            <div class="stats-badge">
              <span>👁 {{ row.viewCount || 0 }}</span>
              <span>❤ {{ row.favoriteCount || 0 }}</span>
              <span>💬 {{ row.commentCount || 0 }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="操作" width="280" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="viewDetail(row)">
              详情
            </el-button>
            <el-button
              v-if="row.status === 3"
              size="small"
              type="success"
              @click="handleApprove(row)"
            >
              通过
            </el-button>
            <el-button
              v-if="row.status === 3"
              size="small"
              type="danger"
              @click="handleReject(row)"
            >
              驳回
            </el-button>
            <el-button
              v-if="row.status === 2 || row.status === 4"
              size="small"
              type="warning"
              @click="handleRestore(row)"
            >
              恢复
            </el-button>
            <el-button
              v-if="row.status === 0 || row.status === 1"
              size="small"
              type="danger"
              @click="handleRemove(row)"
            >
              下架
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

    <!-- 下架对话框 -->
    <el-dialog v-model="removeDialogVisible" title="下架原因" width="500px">
      <el-form>
        <el-form-item label="下架原因">
          <el-input
            v-model="removeReason"
            type="textarea"
            rows="3"
            placeholder="请填写下架原因"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="removeDialogVisible = false">取消</el-button>
        <el-button type="danger" @click="confirmRemove">确认下架</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getAdminProducts,
  getProductStatistics,
  approveProduct,
  rejectProduct,
  removeProduct,
  restoreProduct
} from '@/api/modules/admin'
import { getCategories } from '@/api/modules/category'

const router = useRouter()
const loading = ref(false)
const productList = ref<any[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)
const categories = ref<any[]>([])

const queryParams = reactive({
  keyword: '',
  status: undefined as number | undefined,
  categoryId: undefined as number | undefined
})

const statistics = ref({
  totalProducts: 0,
  availableCount: 0,
  soldCount: 0,
  removedCount: 0,
  pendingReviewCount: 0,
  rejectedCount: 0,
  newProductsToday: 0
})

const rejectDialogVisible = ref(false)
const rejectReason = ref('')
const currentProduct = ref<any>(null)

const removeDialogVisible = ref(false)
const removeReason = ref('')
const removeTarget = ref<any>(null)

// 状态映射
const statusMap = {
  0: { text: '在售', type: 'success' },
  1: { text: '已售', type: 'info' },
  2: { text: '已下架', type: 'danger' },
  3: { text: '待审核', type: 'warning' },
  4: { text: '已驳回', type: 'danger' }
}

const getStatusText = (status: number) => statusMap[status as keyof typeof statusMap]?.text || '未知'
const getStatusType = (status: number) => statusMap[status as keyof typeof statusMap]?.type || 'info'

const loadData = async () => {
  loading.value = true
  try {
    const res = await getAdminProducts({
      ...queryParams,
      page: page.value,
      pageSize: pageSize.value
    })
    productList.value = res.items || []
    total.value = res.totalCount || 0
  } catch (error) {
    ElMessage.error('加载商品列表失败')
  } finally {
    loading.value = false
  }
}

const loadStatistics = async () => {
  try {
    statistics.value = await getProductStatistics()
  } catch (error) {
    console.error('加载统计数据失败', error)
  }
}

const loadCategories = async () => {
  try {
    const res = await getCategories()
    categories.value = res || []
  } catch (error) {
    console.error('加载分类失败', error)
  }
}

const handleSearch = () => {
  page.value = 1
  loadData()
}

const resetSearch = () => {
  queryParams.keyword = ''
  queryParams.status = undefined
  queryParams.categoryId = undefined
  page.value = 1
  loadData()
}

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

const handleRemove = (row: any) => {
  removeTarget.value = row
  removeReason.value = ''
  removeDialogVisible.value = true
}

const confirmRemove = async () => {
  if (!removeReason.value.trim()) {
    ElMessage.warning('请填写下架原因')
    return
  }
  try {
    await removeProduct(removeTarget.value.productId, {
      reason: removeReason.value
    })
    ElMessage.success('已下架')
    removeDialogVisible.value = false
    loadData()
    loadStatistics()
  } catch (error) {
    ElMessage.error('操作失败')
  }
}

const handleRestore = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要恢复商品 "${row.name}" 吗？`, '恢复商品', {
      type: 'warning'
    })
    await restoreProduct(row.productId)
    ElMessage.success('已恢复')
    loadData()
    loadStatistics()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

onMounted(() => {
  loadData()
  loadStatistics()
  loadCategories()
})
</script>

<style scoped>
.product-manage {
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
.filter-card {
  margin-bottom: 20px;
}
.filter-form {
  display: flex;
  flex-wrap: wrap;
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
  object-fit: cover;
}
.product-name {
  font-weight: 500;
}
.product-price {
  color: #f56c6c;
  font-weight: bold;
}
.stats-badge {
  display: flex;
  gap: 12px;
  font-size: 13px;
  color: #666;
}
.table-card {
  margin-top: 20px;
}
.pagination {
  margin-top: 20px;
  justify-content: flex-end;
}
</style>