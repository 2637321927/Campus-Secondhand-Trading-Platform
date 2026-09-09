<template>
  <div class="work-order-manage">
    <el-row :gutter="20" class="stats-row">
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ moderationTasks.totalPending || 0 }}</div>
            <div class="stat-label">待处理工单</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ moderationTasks.waitingCount || 0 }}</div>
            <div class="stat-label">待处理</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ moderationTasks.processingCount || 0 }}</div>
            <div class="stat-label">处理中</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">
              {{ (moderationTasks.reportCount || 0) + (moderationTasks.appealCount || 0) }}
            </div>
            <div class="stat-label">工单总数</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-card class="filter-card">
      <el-form :inline="true" :model="queryParams" class="filter-form">
        <el-form-item label="工单类型">
          <el-select v-model="queryParams.type" class="type-select">
            <el-option label="全部工单" value="all" />
            <el-option label="举报" value="report" />
            <el-option label="申诉" value="appeal" />
          </el-select>
        </el-form-item>
        <el-form-item label="关键词">
          <el-input
            v-model="queryParams.keyword"
            placeholder="原因/说明/用户/商品"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="queryParams.status" placeholder="全部状态" clearable>
            <el-option label="待处理" value="waiting" />
            <el-option label="处理中" value="processing" />
            <el-option label="已完成" value="done" />
          </el-select>
        </el-form-item>
        <el-form-item label="对象">
          <el-select v-model="queryParams.targetType" placeholder="全部对象" clearable>
            <el-option label="商品" value="product" />
            <el-option label="用户" value="user" />
            <el-option label="留言" value="comment" />
            <el-option label="消息" value="message" />
            <el-option label="订单" value="order" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="resetSearch">重置</el-button>
          <el-button :loading="loading" @click="loadData">刷新</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <el-card class="table-card">
      <el-table :data="workOrderList" v-loading="loading" border>
        <el-table-column prop="workOrderId" label="工单ID" width="90" />
        <el-table-column label="类型" width="90">
          <template #default="{ row }">
            <el-tag :type="row.type === 1 ? 'danger' : 'warning'">
              {{ getWorkOrderTypeText(row.type) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="initiatorName" label="发起用户" width="120" />
        <el-table-column label="关联对象" min-width="160">
          <template #default="{ row }">
            <div class="target-cell">
              <el-tag v-if="row.targetType" size="small" type="info">
                {{ getTargetTypeText(row.targetType) }}
              </el-tag>
              <span class="target-name">{{ getTargetName(row) }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="发起原因" min-width="140">
          <template #default="{ row }">{{ getReasonText(row) }}</template>
        </el-table-column>
        <el-table-column prop="info" label="用户说明" min-width="180" show-overflow-tooltip />
        <el-table-column label="状态" width="95">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ getStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="结果" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.result" :type="getResultType(row.result)">
              {{ getResultText(row) }}
            </el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="提交时间" width="165">
          <template #default="{ row }">{{ formatDateTime(row.createTime) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="225" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="openDetail(row)">
              详情
            </el-button>
            <el-button
              v-if="isPending(row)"
              size="small"
              type="danger"
              plain
              @click="handleReject(row)"
            >
              驳回
            </el-button>
            <el-button
              v-if="isPending(row)"
              size="small"
              type="warning"
              @click="openProcessDialog(row)"
            >
              处理
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
        class="pagination"
        @size-change="loadData"
        @current-change="loadData"
      />
    </el-card>

    <el-dialog
      v-model="detailDialogVisible"
      title="工单详情"
      width="760px"
      class="work-order-detail-dialog"
    >
      <div v-loading="detailLoading" class="detail-body">
        <template v-if="selectedOrder">
          <el-descriptions :column="2" border>
            <el-descriptions-item label="工单ID">
              {{ selectedOrder.workOrderId }}
            </el-descriptions-item>
            <el-descriptions-item label="类型">
              {{ getWorkOrderTypeText(selectedOrder.type) }}
            </el-descriptions-item>
            <el-descriptions-item label="发起用户">
              {{ selectedOrder.initiatorName }}（ID：{{ selectedOrder.initiatorId }}）
            </el-descriptions-item>
            <el-descriptions-item label="提交时间">
              {{ formatDateTime(selectedOrder.createTime) }}
            </el-descriptions-item>
            <el-descriptions-item label="对象类型">
              {{ selectedOrder.targetType ? getTargetTypeText(selectedOrder.targetType) : '未关联' }}
            </el-descriptions-item>
            <el-descriptions-item label="对象ID">
              {{ selectedOrder.targetId ?? '未关联' }}
            </el-descriptions-item>
            <el-descriptions-item label="关联商品">
              {{ selectedOrder.productName || '无' }}
            </el-descriptions-item>
            <el-descriptions-item label="被举报人">
              {{ selectedOrder.accusedName || '无' }}
            </el-descriptions-item>
          </el-descriptions>

          <section class="initiated-content">
            <div class="section-title">用户发起内容</div>
            <div class="content-block">
              <div class="content-label">发起原因</div>
              <p class="content-text">{{ getReasonText(selectedOrder) }}</p>
            </div>
            <div class="content-block">
              <div class="content-label">详细说明</div>
              <p class="content-text pre-line">
                {{ selectedOrder.info || '用户未填写详细说明' }}
              </p>
            </div>
            <div v-if="selectedOrder.attachments?.length" class="content-block">
              <div class="content-label">附件（{{ selectedOrder.attachments.length }}）</div>
              <div class="attachment-list">
                <div
                  v-for="attachment in selectedOrder.attachments"
                  :key="attachment.fileId"
                  class="attachment-item"
                >
                  <el-icon class="attachment-icon"><Download /></el-icon>
                  <span class="attachment-name">{{ attachment.fileName }}</span>
                  <el-button
                    size="small"
                    type="primary"
                    plain
                    @click="downloadAttachment(attachment)"
                  >
                    下载
                  </el-button>
                </div>
              </div>
            </div>
            <div v-if="selectedOrder.appealAgainstWorkOrderId" class="content-block">
              <div class="content-label">申诉关联工单</div>
              <p class="content-text">
                #{{ selectedOrder.appealAgainstWorkOrderId }}
                <span v-if="selectedOrder.appealAgainstReason">
                  ：{{ selectedOrder.appealAgainstReason }}
                </span>
              </p>
            </div>
          </section>

          <el-descriptions :column="2" border>
            <el-descriptions-item label="状态">
              {{ getStatusText(selectedOrder.status) }}
            </el-descriptions-item>
            <el-descriptions-item label="结果">
              {{ selectedOrder.result ? getResultText(selectedOrder) : '未处理' }}
            </el-descriptions-item>
            <el-descriptions-item label="处理说明">
              {{ selectedOrder.response || '暂无' }}
            </el-descriptions-item>
            <el-descriptions-item label="处理时间">
              {{ selectedOrder.responseTime ? formatDateTime(selectedOrder.responseTime) : '暂无' }}
            </el-descriptions-item>
          </el-descriptions>

          <section class="timeline-section">
            <div class="section-title">处理记录</div>
            <el-empty
              v-if="!selectedOrder.timeline.length"
              description="暂无处理记录"
              :image-size="64"
            />
            <el-timeline v-else>
              <el-timeline-item
                v-for="item in selectedOrder.timeline"
                :key="item.timelineId"
                :timestamp="formatDateTime(item.createTime)"
                placement="top"
              >
                <div class="timeline-title">{{ getTimelineActionText(item.action) }}</div>
                <p v-if="item.note" class="timeline-note">{{ item.note }}</p>
              </el-timeline-item>
            </el-timeline>
          </section>
        </template>
      </div>
    </el-dialog>

    <el-dialog v-model="processDialogVisible" title="处理工单" width="520px">
      <el-form>
        <el-form-item v-if="currentOrder?.type === 1" label="处理动作">
          <el-select v-model="processAction">
            <el-option label="仅记录处理结果" value="none" />
            <el-option label="下架商品" value="remove_product" />
            <el-option label="恢复商品" value="restore_product" />
            <el-option label="封禁用户" value="ban_user" />
            <el-option label="禁言用户" value="mute_user" />
            <el-option label="限制发布" value="restrict_publish" />
            <el-option label="解除限制" value="unban_user" />
            <el-option label="发送警告" value="warn_user" />
          </el-select>
        </el-form-item>
        <el-form-item label="处理说明">
          <el-input
            v-model="processReason"
            type="textarea"
            :rows="3"
            placeholder="请填写处理说明"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="processDialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="processing" @click="confirmProcess">
          确认处理
        </el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Download } from '@element-plus/icons-vue'
import {
  getModerationTasks,
  getWorkOrderDetail,
  getWorkOrders,
  processWorkOrder,
  rejectWorkOrder
} from '../../../api/modules/admin'
import request from '../../../api/http'
import type {
  AdminWorkOrder,
  AdminWorkOrderAttachment,
  AdminWorkOrderDetail,
  AdminWorkOrderFilterType,
  AdminWorkOrderStatus,
  AdminWorkOrderTargetType,
  ModerationTasks
} from '../../../types/api/admin'
import { getApiErrorMessage } from '../../../utils/error'

const route = useRoute()
const router = useRouter()
const loading = ref(false)
const processing = ref(false)
const workOrderList = ref<AdminWorkOrder[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const moderationTasks = ref<ModerationTasks>({
  totalPending: 0,
  waitingCount: 0,
  processingCount: 0,
  reportCount: 0,
  appealCount: 0,
  recentTasks: []
})

const queryParams = reactive({
  type: 'all' as 'all' | AdminWorkOrderFilterType,
  keyword: '',
  status: undefined as AdminWorkOrderStatus | undefined,
  targetType: undefined as AdminWorkOrderTargetType | undefined
})

const routeType = route.query.type
if (routeType === 'report' || routeType === 'appeal') {
  queryParams.type = routeType
}

const detailDialogVisible = ref(false)
const detailLoading = ref(false)
const selectedOrder = ref<AdminWorkOrderDetail | null>(null)

const processDialogVisible = ref(false)
const currentOrder = ref<AdminWorkOrder | null>(null)
type AdminWorkOrderHandleAction =
  | 'none'
  | 'remove_product'
  | 'restore_product'
  | 'ban_user'
  | 'mute_user'
  | 'restrict_publish'
  | 'unban_user'
  | 'warn_user'
  | 'approve'

const processAction = ref<AdminWorkOrderHandleAction>('none')
const processReason = ref('')

const isPending = (row: AdminWorkOrder) => row.status !== 'done'

const getWorkOrderTypeText = (type: number) => type === 1 ? '举报' : '申诉'

const statusTextMap: Record<AdminWorkOrderStatus, string> = {
  waiting: '待处理',
  processing: '处理中',
  done: '已完成'
}

const statusTypeMap: Record<AdminWorkOrderStatus, 'warning' | 'primary' | 'success'> = {
  waiting: 'warning',
  processing: 'primary',
  done: 'success'
}

const getStatusText = (status: AdminWorkOrderStatus) => statusTextMap[status] || status
const getStatusType = (status: AdminWorkOrderStatus) => statusTypeMap[status] || 'info'

const getResultText = (row: AdminWorkOrder) => {
  if (row.type === 1) {
    return row.result === 'accepted' ? '成立' : row.result === 'rejected' ? '不成立' : '已处理'
  }
  return row.result === 'approved' ? '通过' : row.result === 'rejected' ? '驳回' : '已处理'
}

const getResultType = (result: string) => {
  if (result === 'rejected') return 'danger'
  if (result === 'accepted' || result === 'approved') return 'success'
  return 'warning'
}

const targetTypeTextMap: Record<AdminWorkOrderTargetType, string> = {
  product: '商品',
  user: '用户',
  comment: '留言',
  message: '消息',
  order: '订单'
}

const getTargetTypeText = (type: AdminWorkOrderTargetType) => targetTypeTextMap[type] || type

const getTargetName = (row: AdminWorkOrder) => {
  if (row.productName) return row.productName
  if (row.accusedName) return row.accusedName
  return row.targetId ? `ID ${row.targetId}` : '未关联'
}

const reasonTextMap: Record<string, string> = {
  fraud: '欺诈或虚假信息',
  counterfeit: '假冒伪劣',
  illegal: '违禁或违法内容',
  harassment: '骚扰或恶意行为',
  spam: '骚扰或垃圾信息',
  product_removed: '商品被下架',
  account_restricted: '账号受限',
  report_result: '举报处理结果',
  other: '其他'
}

const getReasonText = (row: Pick<AdminWorkOrder, 'reason'>) => reasonTextMap[row.reason] || row.reason

const timelineActionTextMap: Record<string, string> = {
  create: '用户发起',
  accept: '举报成立',
  approve: '申诉通过',
  reject: '工单驳回',
  handle: '工单处理',
  reply: '管理员回复'
}

const getTimelineActionText = (action: string) => timelineActionTextMap[action] || action

const loadData = async () => {
  loading.value = true
  try {
    const response = await getWorkOrders({
      keyword: queryParams.keyword.trim() || undefined,
      status: queryParams.status,
      type: queryParams.type === 'all' ? undefined : queryParams.type,
      targetType: queryParams.targetType,
      page: page.value,
      pageSize: pageSize.value
    })
    workOrderList.value = response.data.items || []
    total.value = response.data.totalCount || 0
  } catch (error) {
    workOrderList.value = []
    ElMessage.error(getApiErrorMessage(error, '加载工单列表失败'))
  } finally {
    loading.value = false
  }
}

const loadTasks = async () => {
  try {
    moderationTasks.value = (await getModerationTasks()).data
  } catch (error) {
    console.error('加载工单统计失败：', error)
  }
}

const handleSearch = () => {
  page.value = 1
  loadData()
}

const resetSearch = () => {
  queryParams.type = 'all'
  queryParams.keyword = ''
  queryParams.status = undefined
  queryParams.targetType = undefined
  page.value = 1
  loadData()
}

const openDetail = async (row: AdminWorkOrder) => {
  await openDetailById(row.workOrderId)
}

const openDetailById = async (workOrderId: number) => {
  detailDialogVisible.value = true
  detailLoading.value = true
  selectedOrder.value = null
  try {
    const response = await getWorkOrderDetail(workOrderId)
    selectedOrder.value = response.data
  } catch (error) {
    ElMessage.error(getApiErrorMessage(error, '加载工单详情失败'))
    detailDialogVisible.value = false
  } finally {
    detailLoading.value = false
  }
}

const downloadAttachment = async (attachment: AdminWorkOrderAttachment) => {
  try {
    const response = await request.get(`/api/files/${attachment.fileId}`, {
      responseType: 'blob'
    })
    const url = URL.createObjectURL(response.data as Blob)
    const link = document.createElement('a')
    link.href = url
    link.download = attachment.fileName
    link.click()
    URL.revokeObjectURL(url)
  } catch (error) {
    ElMessage.error(getApiErrorMessage(error, '附件下载失败'))
  }
}

const isUserCancelled = (error: unknown) => error === 'cancel'

const handleReject = async (row: AdminWorkOrder) => {
  const typeName = getWorkOrderTypeText(row.type)
  try {
    await ElMessageBox.confirm(`确定驳回这条${typeName}工单吗？`, `${typeName}驳回`, {
      confirmButtonText: '驳回',
      cancelButtonText: '取消',
      type: 'warning'
    })
  } catch (error) {
    if (!isUserCancelled(error)) {
      ElMessage.error(getApiErrorMessage(error, '操作失败'))
    }
    return
  }

  try {
    await rejectWorkOrder(row.workOrderId)
    ElMessage.success(`已驳回${typeName}工单`)
    loadData()
    loadTasks()
  } catch (error) {
    ElMessage.error(getApiErrorMessage(error, '驳回失败'))
  }
}

const openProcessDialog = (row: AdminWorkOrder) => {
  currentOrder.value = row
  processAction.value = row.type === 2 ? 'approve' : 'none'
  processReason.value = ''
  processDialogVisible.value = true
}

const confirmProcess = async () => {
  if (!currentOrder.value) return
  if (!processReason.value.trim()) {
    ElMessage.warning('请填写处理说明')
    return
  }

  processing.value = true
  try {
    await processWorkOrder(currentOrder.value.workOrderId, {
      action: currentOrder.value.type === 2 ? 'approve' : processAction.value,
      reason: processReason.value.trim()
    })
    ElMessage.success('工单处理成功')
    processDialogVisible.value = false
    loadData()
    loadTasks()
  } catch (error) {
    ElMessage.error(getApiErrorMessage(error, '工单处理失败'))
  } finally {
    processing.value = false
  }
}

const formatDateTime = (value: string) => {
  if (!value) return '-'
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return date.toLocaleString('zh-CN')
}

onMounted(async () => {
  const focusId = Number(route.query.focusId)
  await loadData()
  await loadTasks()

  if (focusId > 0) {
    await openDetailById(focusId)
    void router.replace({ query: { ...route.query, focusId: undefined } })
  }
})
</script>

<style scoped>
.work-order-manage {
  padding: 4px 0;
}

.stats-row {
  margin-bottom: 20px;
}

.stat-item {
  text-align: center;
}

.stat-number {
  color: #24735b;
  font-size: 28px;
  font-weight: bold;
}

.stat-label {
  margin-top: 5px;
  color: #666;
}

.filter-card {
  margin-bottom: 20px;
}

.filter-form {
  align-items: center;
  flex-wrap: wrap;
}

.type-select {
  width: 130px;
}

.table-card {
  margin-top: 4px;
}

.target-cell {
  align-items: center;
  display: flex;
  gap: 8px;
  min-width: 0;
}

.target-name {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.pagination {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

.detail-body {
  min-height: 160px;
}

.initiated-content,
.timeline-section {
  border: 1px solid #dbe6e1;
  border-radius: 6px;
  margin: 18px 0;
  padding: 16px;
}

.section-title {
  color: #1e2a26;
  font-size: 16px;
  font-weight: 600;
  margin-bottom: 14px;
}

.content-block + .content-block {
  margin-top: 12px;
}

.content-label {
  color: #64706b;
  font-size: 13px;
  margin-bottom: 4px;
}

.content-text {
  color: #26332e;
  line-height: 1.7;
  margin: 0;
  word-break: break-word;
}

.content-text.pre-line {
  white-space: pre-line;
}

.timeline-note {
  color: #5b6661;
  line-height: 1.6;
  margin: 4px 0 0;
  word-break: break-word;
}

.attachment-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-top: 8px;
}

.attachment-item {
  align-items: center;
  background: #f7faf9;
  border: 1px solid #e2e8e6;
  border-radius: 6px;
  display: flex;
  gap: 10px;
  min-width: 0;
  padding: 8px 12px;
}

.attachment-icon {
  color: #24735b;
  flex: 0 0 auto;
}

.attachment-name {
  color: #333;
  flex: 1;
  min-width: 0;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.timeline-title {
  color: #1e2a26;
  font-weight: 600;
}
</style>
