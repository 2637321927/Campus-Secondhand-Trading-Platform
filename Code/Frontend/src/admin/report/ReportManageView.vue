// 举报管理
<template>
  <div class="report-manage">
    <!-- 统计卡片 -->
    <el-row :gutter="20" class="stats-row">
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ moderationTasks.totalPending || 0 }}</div>
            <div class="stat-label">待处理总数</div>
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
            <div class="stat-number">{{ moderationTasks.reportCount || 0 }}</div>
            <div class="stat-label">举报总数</div>
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
            placeholder="原因/描述/昵称/商品名"
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
        <el-form-item label="举报类型">
          <el-select v-model="queryParams.targetType" placeholder="全部类型" clearable>
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
          <el-button @click="loadData" :loading="loading">刷新</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 举报列表 -->
    <el-card class="table-card">
      <el-table :data="reportList" v-loading="loading" border>
        <el-table-column prop="reportId" label="ID" width="70" />
        <el-table-column prop="reporterName" label="举报人" width="100" />
        <el-table-column label="举报对象" min-width="150">
          <template #default="{ row }">
            <div>
              <el-tag size="small" type="info">{{ row.targetType }}</el-tag>
              <span>{{ row.targetName }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="reason" label="原因" min-width="150" />
        <el-table-column prop="description" label="描述" min-width="150" show-overflow-tooltip />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ getStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="结果" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.result === 'accepted'" type="success">成立</el-tag>
            <el-tag v-else-if="row.result === 'rejected'" type="info">不成立</el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="举报时间" width="160">
          <template #default="{ row }">{{ formatDate(row.createTime) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="240" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="viewDetail(row)">
              详情
            </el-button>
            <el-button
              v-if="row.status === 'waiting' || row.status === 'processing'"
              size="small"
              type="success"
              @click="handleAccept(row)"
            >
              成立
            </el-button>
            <el-button
              v-if="row.status === 'waiting' || row.status === 'processing'"
              size="small"
              type="info"
              @click="handleReject(row)"
            >
              驳回
            </el-button>
            <el-button
              v-if="row.status === 'waiting' || row.status === 'processing'"
              size="small"
              type="warning"
              @click="handleAction(row)"
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
        @size-change="loadData"
        @current-change="loadData"
        class="pagination"
      />
    </el-card>

    <!-- 综合处理对话框 -->
    <el-dialog v-model="actionDialogVisible" title="综合处理" width="500px">
      <el-form>
        <el-form-item label="处理动作">
          <el-select v-model="actionType" placeholder="请选择处理动作">
            <el-option label="仅记录" value="none" />
            <el-option label="下架商品" value="remove_product" />
            <el-option label="恢复商品" value="restore_product" />
            <el-option label="封禁用户" value="ban_user" />
            <el-option label="禁言用户" value="mute_user" />
            <el-option label="限制发布" value="restrict_publish" />
            <el-option label="解除限制" value="unban_user" />
            <el-option label="发送警告" value="warn_user" />
          </el-select>
        </el-form-item>
        <el-form-item label="处理原因">
          <el-input
            v-model="actionReason"
            type="textarea"
            rows="3"
            placeholder="请填写处理原因"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="actionDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="confirmAction">确认处理</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getReports,
  getModerationTasks,
  acceptReport,
  rejectReport,
  handleReport
} from '@/api/modules/admin'

const router = useRouter()
const loading = ref(false)
const reportList = ref<any[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const queryParams = reactive({
  keyword: '',
  status: undefined as string | undefined,
  targetType: undefined as string | undefined
})

const moderationTasks = ref({
  totalPending: 0,
  waitingCount: 0,
  processingCount: 0,
  reportCount: 0,
  appealCount: 0,
  recentTasks: []
})

// 状态映射
const statusMap = {
  waiting: { text: '待处理', type: 'warning' },
  processing: { text: '处理中', type: 'primary' },
  done: { text: '已完成', type: 'success' }
}

const getStatusText = (status: string) => statusMap[status as keyof typeof statusMap]?.text || '未知'
const getStatusType = (status: string) => statusMap[status as keyof typeof statusMap]?.type || 'info'

// 综合处理
const actionDialogVisible = ref(false)
const actionType = ref('')
const actionReason = ref('')
const currentReport = ref<any>(null)

const loadData = async () => {
  loading.value = true
  try {
    const res = await getReports({
      ...queryParams,
      page: page.value,
      pageSize: pageSize.value
    })
    reportList.value = res.items || []
    total.value = res.totalCount || 0
  } catch (error) {
    ElMessage.error('加载举报列表失败')
  } finally {
    loading.value = false
  }
}

const loadTasks = async () => {
  try {
    moderationTasks.value = await getModerationTasks()
  } catch (error) {
    console.error('加载任务统计失败', error)
  }
}

const handleSearch = () => {
  page.value = 1
  loadData()
}

const resetSearch = () => {
  queryParams.keyword = ''
  queryParams.status = undefined
  queryParams.targetType = undefined
  page.value = 1
  loadData()
}

const viewDetail = (row: any) => {
  router.push(`/admin/reports/${row.reportId}`)
}

const handleAccept = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定认定此举报成立吗？`, '举报成立', {
      type: 'success'
    })
    await acceptReport(row.reportId)
    ElMessage.success('已认定举报成立')
    loadData()
    loadTasks()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

const handleReject = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定驳回此举报吗？`, '举报驳回', {
      type: 'warning'
    })
    await rejectReport(row.reportId)
    ElMessage.success('已驳回举报')
    loadData()
    loadTasks()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

const handleAction = (row: any) => {
  currentReport.value = row
  actionType.value = ''
  actionReason.value = ''
  actionDialogVisible.value = true
}

const confirmAction = async () => {
  if (!actionType.value) {
    ElMessage.warning('请选择处理动作')
    return
  }
  if (!actionReason.value.trim()) {
    ElMessage.warning('请填写处理原因')
    return
  }
  try {
    await handleReport(currentReport.value.reportId, {
      action: actionType.value as any,
      reason: actionReason.value
    })
    ElMessage.success('处理成功')
    actionDialogVisible.value = false
    loadData()
    loadTasks()
  } catch (error) {
    ElMessage.error('操作失败')
  }
}

const formatDate = (date: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('zh-CN')
}

onMounted(() => {
  loadData()
  loadTasks()
})
</script>

<style scoped>
.report-manage {
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
.table-card {
  margin-top: 20px;
}
.pagination {
  margin-top: 20px;
  justify-content: flex-end;
}
</style>