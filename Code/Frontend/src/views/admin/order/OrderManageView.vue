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
            <el-option label="待确认" value="pending" />
            <el-option label="已确认" value="confirmed" />
            <el-option label="已发货" value="shipped" />
            <el-option label="已完成" value="completed" />
            <el-option label="已取消" value="cancelled" />
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
                :src="row.productImage || '/default-image.png'"
                class="product-thumb"
                fit="cover"
              />
              <div>
                <div class="product-name">{{ row.productName }}</div>
                <div class="product-price">¥{{ row.totalAmount }}</div>
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
              v-if="row.status === 'pending' || row.status === 'confirmed'"
              size="small"
              type="warning"
              @click="handleCancel(row)"
            >
              取消
            </el-button>
            <el-button
              v-if="row.status === 'shipped'"
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
          <el-descriptions-item label="总金额">¥{{ currentOrder.totalAmount }}</el-descriptions-item>
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

// 状态映射
const statusMap: Record<string, { text: string; type: string }> = {
  pending: { text: '待确认', type: 'warning' },
  confirmed: { text: '已确认', type: 'primary' },
  shipped: { text: '已发货', type: 'info' },
  completed: { text: '已完成', type: 'success' },
  cancelled: { text: '已取消', type: 'danger' }
}

const getStatusText = (status: string) => statusMap[status]?.text || '未知'
const getStatusType = (status: string) => statusMap[status]?.type || 'info'

// 模拟数据
const mockOrders = [
  {
    orderId: 'ORD20260902001',
    productName: 'iPhone 15 Pro Max',
    productImage: '',
    totalAmount: 6999,
    buyerName: '张三',
    sellerName: '李四',
    status: 'pending',
    paymentMethod: '微信支付',
    createTime: new Date().toISOString(),
    shippingAddress: '广东省广州市天河区xx路xx号',
    timeline: [
      { id: 1, content: '订单创建', type: 'primary', createTime: new Date().toISOString() }
    ]
  },
  {
    orderId: 'ORD20260902002',
    productName: 'MacBook Pro 14寸',
    productImage: '',
    totalAmount: 12999,
    buyerName: '王五',
    sellerName: '赵六',
    status: 'shipped',
    paymentMethod: '支付宝',
    createTime: new Date(Date.now() - 86400000).toISOString(),
    shippingAddress: '北京市海淀区中关村xx号',
    timeline: [
      { id: 1, content: '订单创建', type: 'primary', createTime: new Date(Date.now() - 86400000).toISOString() },
      { id: 2, content: '卖家确认', type: 'success', createTime: new Date(Date.now() - 43200000).toISOString() },
      { id: 3, content: '商品已发货', type: 'info', createTime: new Date(Date.now() - 21600000).toISOString() }
    ]
  }
]

const loadData = async () => {
  loading.value = true
  try {
    // TODO: 替换为真实 API
    // const res = await getOrderList({ ...queryParams, page: page.value, pageSize: pageSize.value })
    // orderList.value = res.items
    // total.value = res.totalCount

    // 模拟数据
    await new Promise(resolve => setTimeout(resolve, 500))
    orderList.value = mockOrders
    total.value = 2

    // 模拟统计
    statistics.value = {
      totalOrders: 25,
      pendingCount: 5,
      shippingCount: 8,
      completedCount: 10
    }
  } catch (error) {
    ElMessage.error('加载订单列表失败')
  } finally {
    loading.value = false
  }
}

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

const viewDetail = (row: any) => {
  currentOrder.value = row
  detailDialogVisible.value = true
}

const handleCancel = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要取消订单 "${row.orderId}" 吗？`, '取消订单', {
      type: 'warning'
    })
    // TODO: 调用取消订单 API
    // await cancelOrder(row.orderId)
    ElMessage.success('订单已取消')
    loadData()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

const handleComplete = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要完成订单 "${row.orderId}" 吗？`, '完成订单', {
      type: 'success'
    })
    // TODO: 调用完成订单 API
    // await completeOrder(row.orderId)
    ElMessage.success('订单已完成')
    loadData()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

const formatDate = (date: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('zh-CN')
}

onMounted(() => {
  loadData()
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
  object-fit: cover;
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