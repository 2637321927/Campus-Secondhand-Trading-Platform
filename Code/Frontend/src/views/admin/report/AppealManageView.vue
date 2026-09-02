// 申诉管理
<template>
  <div class="appeal-manage">
    <!-- 统计卡片 -->
    <el-row :gutter="20" class="stats-row">
      <el-col :span="8">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ moderationTasks.totalPending || 0 }}</div>
            <div class="stat-label">待处理总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ moderationTasks.appealCount || 0 }}</div>
            <div class="stat-label">申诉总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ moderationTasks.waitingCount || 0 }}</div>
            <div class="stat-label">待处理申诉</div>
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
            placeholder="申诉内容/用户名"
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
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="resetSearch">重置</el-button>
          <el-button @click="loadData" :loading="loading">刷新</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 申诉列表 -->
    <el-card class="table-card">
      <el-table :data="appealList" v-loading="loading" border>
        <el-table-column prop="appealId" label="ID" width="70" />
        <el-table-column prop="userName" label="申诉人" width="120" />
        <el-table-column prop="content" label="申诉内容" min-width="200" show-overflow-tooltip />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ getStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="结果" width="100">
          <template #default="{ row }">
            <el-tag v-if="row.result === 'approved'" type="success">通过</el-tag>
            <el-tag v-else-if="row.result === 'rejected'" type="danger">驳回</el-tag>
            <span v-else>-</span>
          </template>
        </el-table-column>
        <el-table-column label="提交时间" width="160">
          <template #default="{ row }">{{ formatDate(row.createTime) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="280" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="viewDetail(row)">
              详情
            </el-button>
            <el-button
              v-if="row.status === 'waiting' || row.status === 'processing'"
              size="small"
              type="success"
              @click="handleApprove(row)"
            >
              通过
            </el-button>
            <el-button
              v-if="row.status === 'waiting' || row.status === 'processing'"
              size="small"
              type="danger"
              @click="handleReject(row)"
            >
              驳回
            </el-button>
            <el-button
              v-if="row.status === 'waiting' || row.status === 'processing'"
              size="small"
              type="warning"
              @click="openReplyDialog(row)"
            >
              回复
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

    <!-- 回复对话框 -->
    <el-dialog v-model="replyDialogVisible" title="回复申诉" width="500px">
      <el-form>
        <el-form-item label="回复内容">
          <el-input
            v-model="replyContent"
            type="textarea"
            rows="4"
            placeholder="请填写回复内容"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="replyDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="confirmReply">发送回复</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getAppeals,
  getModerationTasks,
  approveAppeal,
  rejectAppeal,
  replyAppeal
} from '../../../api/modules/admin' 

const router = useRouter()
const loading = ref(false)
const appealList = ref<any[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const queryParams = reactive({
  keyword: '',
  status: undefined as string | undefined
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

// 回复
const replyDialogVisible = ref(false)
const replyContent = ref('')
const currentAppeal = ref<any>(null)

const loadData = async () => {
  loading.value = true
  try {
    const res = await getAppeals({
      ...queryParams,
      page: page.value,
      pageSize: pageSize.value
    })
    appealList.value = res.items || []
    total.value = res.totalCount || 0
  } catch (error) {
    ElMessage.error('加载申诉列表失败')
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
  page.value = 1
  loadData()
}

const viewDetail = (row: any) => {
  router.push(`/admin/appeals/${row.appealId}`)
}

const handleApprove = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定通过此申诉吗？`, '申诉通过', {
      type: 'success'
    })
    await approveAppeal(row.appealId)
    ElMessage.success('申诉已通过')
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
    await ElMessageBox.confirm(`确定驳回此申诉吗？`, '申诉驳回', {
      type: 'warning'
    })
    await rejectAppeal(row.appealId)
    ElMessage.success('已驳回申诉')
    loadData()
    loadTasks()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

const openReplyDialog = (row: any) => {
  currentAppeal.value = row
  replyContent.value = ''
  replyDialogVisible.value = true
}

const confirmReply = async () => {
  if (!replyContent.value.trim()) {
    ElMessage.warning('请填写回复内容')
    return
  }
  try {
    await replyAppeal(currentAppeal.value.appealId, {
      reply: replyContent.value
    })
    ElMessage.success('回复已发送')
    replyDialogVisible.value = false
    loadData()
  } catch (error) {
    ElMessage.error('发送失败')
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
.appeal-manage {
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