<template>
  <div class="order-manage">
    <!-- 统计卡片 -->
    <el-row :gutter="20" class="stats-row">
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.totalOrders || 0 }}</div>
            <div class="stat-label">订单总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.pendingCount || 0 }}</div>
            <div class="stat-label">待处理</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.shippingCount || 0 }}</div>
            <div class="stat-label">配送中</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.completedCount || 0 }}</div>
            <div class="stat-label">已完成</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 筛选栏 -->
    <el-card class="filter-card">
      <el-form :inline="true" :model="queryParams" class="filter-form">
        <el-form-item label="订单号">
          <el-input
            v-model="queryParams.orderId"
            placeholder="请输入订单号"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="queryParams.status" placeholder="全部状态" clearable>
            <!-- 根据对接说明，使用后端状态枚举 -->
            <el-option label="待付款" value="pending" />
            <el-option label="已付款" value="paid" />
            <el-option label="已确认" value="confirmed" />
            <el-option label="已发货" value="shipping" />
            <el-option label="已完成" value="success" />
            <el-option label="已取消" value="cancel" />
            <el-option label="退款中" value="refund" />
          </el-select>
        </el-form-item>
        <el-form-item label="时间范围">
          <el-date-picker
            v-model="queryParams.dateRange"
            type="daterange"
            start-placeholder="开始日期"
            end-placeholder="结束日期"
            format="YYYY-MM-DD"
            value-format="YYYY-MM-DD"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="resetSearch">重置</el-button>
          <el-button @click="loadData" :loading="loading">刷新</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 订单列表 -->
    <el-card class="table-card">
      <el-table :data="orderList" v-loading="loading" border>
        <el-table-column prop="orderId" label="订单号" width="160" />
        <el-table-column label="商品信息" min-width="180">
          <template #default="{ row }">
            <div class="product-info">
              <el-image
                :src="getProductImageUrl(row.productCoverImageId)"
                class="product-thumb"
                fit="cover"
              >
                <template #error>
                  <div class="image-placeholder">暂无图片</div>
                </template>
              </el-image>
              <div>
                <div class="product-name">{{ row.productName }}</div>
                <div class="product-price">¥{{ Number(row.totalAmount).toFixed(2) }}</div>
              </div>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="buyerName" label="买家" width="100" />
        <el-table-column prop="sellerName" label="卖家" width="100" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ getStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="支付方式" width="100">
          <template #default="{ row }">
            {{ row.paymentMethod || '在线支付' }}
          </template>
        </el-table-column>
        <el-table-column label="下单时间" width="160">
          <template #default="{ row }">
            {{ formatDate(row.createTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="200" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="viewDetail(row)">
              查看
            </el-button>
            <el-button
              v-if="row.status === 'pending' || row.status === 'paid' || row.status === 'confirmed'"
              size="small"
              type="warning"
              @click="handleCancel(row)"
            >
              取消
            </el-button>
            <el-button
              v-if="row.status === 'shipping'"
              size="small"
              type="success"
              @click="handleComplete(row)"
            >
              完成
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

    <!-- 订单详情对话框 -->
    <el-dialog v-model="detailDialogVisible" title="订单详情" width="700px">
      <div v-if="currentOrder">
        <el-descriptions :column="2" border>
          <el-descriptions-item label="订单号">{{ currentOrder.orderId }}</el-descriptions-item>
          <el-descriptions-item label="状态">
            <el-tag :type="getStatusType(currentOrder.status)">
              {{ getStatusText(currentOrder.status) }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="商品名称">{{ currentOrder.productName }}</el-descriptions-item>
          <el-descriptions-item label="总金额">¥{{ Number(currentOrder.totalAmount).toFixed(2) }}</el-descriptions-item>
          <el-descriptions-item label="买家">{{ currentOrder.buyerName }}</el-descriptions-item>
          <el-descriptions-item label="卖家">{{ currentOrder.sellerName }}</el-descriptions-item>
          <el-descriptions-item label="支付方式">{{ currentOrder.paymentMethod || '在线支付' }}</el-descriptions-item>
          <el-descriptions-item label="下单时间">{{ formatDate(currentOrder.createTime) }}</el-descriptions-item>
          <el-descriptions-item label="收货地址" :span="2">
            {{ currentOrder.shippingAddress || '未填写' }}
          </el-descriptions-item>
        </el-descriptions>

        <!-- 订单时间线 -->
        <div v-if="currentOrder.timeline?.length" class="timeline-section">
          <h4>订单状态流转</h4>
          <el-timeline>
            <el-timeline-item
              v-for="item in currentOrder.timeline"
              :key="item.id"
              :timestamp="formatDate(item.createTime)"
              :type="item.type"
            >
              {{ item.content }}
            </el-timeline-item>
          </el-timeline>
        </div>
      </div>
      <template #footer>
        <el-button @click="detailDialogVisible = false">关闭</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getOrderList, getOrderStatistics, cancelOrder, completeOrder } from '../../../api/modules/admin'

const loading = ref(false)
const orderList = ref<any[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const detailDialogVisible = ref(false)
const currentOrder = ref<any>(null)

const queryParams = reactive({
  orderId: '',
  status: undefined as string | undefined,
  dateRange: [] as string[]
})

const statistics = ref({
  totalOrders: 0,
  pendingCount: 0,
  shippingCount: 0,
  completedCount: 0
})

// ========== 获取商品图片 URL ==========
// 根据对接说明，订单列表使用 productCoverImageId
const getProductImageUrl = (fileId: number | null | undefined) => {
  if (!fileId) return '/default-product.png'
  return `/api/files/${fileId}`
}

// ========= 状态映射 ==========
const statusMap: Record<string, { text: string; type: string }> = {
  pending: { text: '待付款', type: 'warning' },
  paid: { text: '已付款', type: 'primary' },
  confirmed: { text: '已确认', type: 'primary' },
  shipping: { text: '已发货', type: 'info' },
  success: { text: '已完成', type: 'success' },
  cancel: { text: '已取消', type: 'danger' },
  refund: { text: '退款中', type: 'warning' }
}

const getStatusText = (status: string) => statusMap[status]?.text || status || '未知'
const getStatusType = (status: string) => statusMap[status]?.type || 'info'

// ========== 加载数据 ==========
const loadData = async () => {
  loading.value = true
  try {
    const params: any = {
      page: page.value,
      pageSize: pageSize.value
    }
    if (queryParams.orderId) params.orderId = queryParams.orderId
    if (queryParams.status) params.status = queryParams.status
    if (queryParams.dateRange?.length === 2) {
      params.startDate = queryParams.dateRange[0]
      params.endDate = queryParams.dateRange[1]
    }

    const res = await getOrderList(params)
    const data = res.data
    orderList.value = data.items || []
    total.value = data.totalCount || 0
  } catch (error: any) {
    console.error('加载订单列表失败:', error)
    ElMessage.error(error?.message || '加载订单列表失败')
  } finally {
    loading.value = false
  }
}

// ========== 加载统计数据 ==========
const loadStatistics = async () => {
  try {
    const res = await getOrderStatistics()
    statistics.value = res.data
  } catch (error) {
    console.error('加载统计数据失败', error)
  }
}

// ========== 搜索和重置 ==========
const handleSearch = () => {
  page.value = 1
  loadData()
}

const resetSearch = () => {
  queryParams.orderId = ''
  queryParams.status = undefined
  queryParams.dateRange = []
  page.value = 1
  loadData()
}

// ========== 查看详情 ==========
const viewDetail = async (row: any) => {
  try {
    currentOrder.value = row
    detailDialogVisible.value = true
  } catch (error) {
    ElMessage.error('加载订单详情失败')
  }
}

// ========== 取消订单 ==========
const handleCancel = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要取消订单 "${row.orderId}" 吗？`, '取消订单', {
      type: 'warning'
    })
    await cancelOrder(row.orderId)
    ElMessage.success('订单已取消')
    loadData()
    loadStatistics()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

// ========== 完成订单 ==========
const handleComplete = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要完成订单 "${row.orderId}" 吗？`, '完成订单', {
      type: 'success'
    })
    await completeOrder(row.orderId)
    ElMessage.success('订单已完成')
    loadData()
    loadStatistics()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

// ========== 工具函数 ==========
const formatDate = (date: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('zh-CN')
}

// ========== 生命周期 ==========
onMounted(() => {
  loadData()
  loadStatistics()
})
</script>

<style scoped>
.order-manage {
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
.product-thumb {
  width: 50px;
  height: 50px;
  border-radius: 8px;
  flex-shrink: 0;
  background: #f5f7f6;
  object-fit: cover;
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
.timeline-section {
  margin-top: 20px;
}
.timeline-section h4 {
  margin-bottom: 12px;
  color: #333;
}
</style>