// 用户管理界面
<template>
  <div class="user-manage">
    <!-- 统计卡片 -->
    <el-row :gutter="20" class="stats-row">
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ userStats.totalUsers }}</div>
            <div class="stat-label">用户总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ userStats.normalUsers }}</div>
            <div class="stat-label">正常</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ userStats.mutedUsers }}</div>
            <div class="stat-label">禁言</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ userStats.publishRestrictedUsers }}</div>
            <div class="stat-label">限制发布</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ userStats.bannedUsers }}</div>
            <div class="stat-label">已封禁</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="4">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ userStats.usersWithProducts }}</div>
            <div class="stat-label">已发布商品</div>
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
            placeholder="邮箱/手机号/昵称"
            clearable
            @keyup.enter="handleSearch"
          />
        </el-form-item>
        <el-form-item label="状态">
          <el-select v-model="queryParams.accountStatus" placeholder="全部状态" clearable>
            <el-option label="正常" :value="0" />
            <el-option label="禁言" :value="1" />
            <el-option label="限制发布" :value="2" />
            <el-option label="封禁" :value="3" />
          </el-select>
        </el-form-item>
        <el-form-item label="用户类型">
          <el-select v-model="queryParams.userType" placeholder="全部" clearable>
            <el-option label="普通用户" :value="0" />
            <el-option label="管理员" :value="1" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSearch">搜索</el-button>
          <el-button @click="resetSearch">重置</el-button>
          <el-button @click="loadData" :loading="loading">刷新</el-button>
        </el-form-item>
      </el-form>
    </el-card>

    <!-- 用户列表 -->
    <el-card class="table-card">
      <el-table :data="userList" v-loading="loading" border>
        <el-table-column prop="userId" label="ID" width="70" />
        <el-table-column prop="userName" label="昵称" width="120" />
        <el-table-column prop="email" label="邮箱" min-width="180" />
        <el-table-column prop="phoneNumber" label="手机号" width="130" />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.accountStatus)">
              {{ getStatusText(row.accountStatus) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="信誉分" width="80">
          <template #default="{ row }">
            <span :style="{ color: row.credit >= 80 ? '#67c23a' : '#f56c6c' }">
              {{ row.credit }}
            </span>
          </template>
        </el-table-column>
        <el-table-column label="数据" width="180">
          <template #default="{ row }">
            <div class="stats-badge">
              <span>📦 {{ row.productCount || 0 }}</span>
              <span>📋 {{ row.orderCount || 0 }}</span>
              <span>⚠️ {{ row.warningCount || 0 }}</span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="注册时间" width="160">
          <template #default="{ row }">
            {{ formatDate(row.registerTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="220" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="viewDetail(row)">
              详情
            </el-button>
            <el-button
              size="small"
              :type="row.accountStatus === 0 ? 'warning' : 'success'"
              @click="toggleStatus(row)"
            >
              {{ row.accountStatus === 0 ? '封禁' : '解封' }}
            </el-button>
            <el-button size="small" type="danger" @click="sendWarning(row)">
              警告
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

    <!-- 状态修改对话框 -->
    <el-dialog v-model="statusDialogVisible" :title="statusDialogTitle" width="500px">
      <el-form>
        <el-form-item label="操作原因">
          <el-input
            v-model="statusReason"
            type="textarea"
            rows="3"
            placeholder="请填写操作原因"
          />
        </el-form-item>
        <el-form-item v-if="newStatus === 3" label="封禁截止时间">
          <el-date-picker
            v-model="bannedUntil"
            type="datetime"
            placeholder="选择封禁截止时间（不选则永久封禁）"
            format="YYYY-MM-DD HH:mm:ss"
            value-format="YYYY-MM-DDTHH:mm:ss"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="statusDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="confirmStatusChange">确认</el-button>
      </template>
    </el-dialog>

    <!-- 警告对话框 -->
    <el-dialog v-model="warningDialogVisible" title="发送警告" width="500px">
      <el-form>
        <el-form-item label="警告内容">
          <el-input
            v-model="warningReason"
            type="textarea"
            rows="3"
            placeholder="请填写警告内容"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="warningDialogVisible = false">取消</el-button>
        <el-button type="danger" @click="confirmWarning">发送</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
  getAdminUsers,
  getUserStatistics,
  updateUserStatus,
  sendUserWarning
} from '../../../api/modules/admin' 

const router = useRouter()
const loading = ref(false)
const userList = ref<any[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const queryParams = reactive({
  keyword: '',
  accountStatus: undefined as number | undefined,
  userType: undefined as number | undefined
})

const userStats = ref({
  totalUsers: 0,
  normalUsers: 0,
  mutedUsers: 0,
  publishRestrictedUsers: 0,
  bannedUsers: 0,
  usersWithProducts: 0,
  totalOrders: 0,
  totalWarnings: 0
})

// 状态映射
const statusMap = {
  0: { text: '正常', type: 'success' },
  1: { text: '禁言', type: 'warning' },
  2: { text: '限制发布', type: 'warning' },
  3: { text: '封禁', type: 'danger' }
}

const getStatusText = (status: number) => statusMap[status as keyof typeof statusMap]?.text || '未知'
const getStatusType = (status: number) => statusMap[status as keyof typeof statusMap]?.type || 'info'

// 状态修改
const statusDialogVisible = ref(false)
const statusDialogTitle = ref('')
const statusReason = ref('')
const statusTarget = ref<any>(null)
const newStatus = ref<number>(0)
const bannedUntil = ref<string | null>(null)

// 警告
const warningDialogVisible = ref(false)
const warningReason = ref('')
const warningTarget = ref<any>(null)

const loadData = async () => {
  loading.value = true
  try {
    const res = await getAdminUsers({
      ...queryParams,
      page: page.value,
      pageSize: pageSize.value
    })
    userList.value = res.items || []
    total.value = res.totalCount || 0
  } catch (error) {
    ElMessage.error('加载用户列表失败')
  } finally {
    loading.value = false
  }
}

const loadStatistics = async () => {
  try {
    userStats.value = await getUserStatistics()
  } catch (error) {
    console.error('加载统计数据失败', error)
  }
}

const handleSearch = () => {
  page.value = 1
  loadData()
}

const resetSearch = () => {
  queryParams.keyword = ''
  queryParams.accountStatus = undefined
  queryParams.userType = undefined
  page.value = 1
  loadData()
}

const viewDetail = (row: any) => {
  router.push(`/admin/users/${row.userId}`)
}

const toggleStatus = (row: any) => {
  statusTarget.value = row
  if (row.accountStatus === 0) {
    statusDialogTitle.value = `封禁用户 "${row.userName}"`
    newStatus.value = 3
  } else {
    statusDialogTitle.value = `解封用户 "${row.userName}"`
    newStatus.value = 0
  }
  statusReason.value = ''
  bannedUntil.value = null
  statusDialogVisible.value = true
}

const confirmStatusChange = async () => {
  if (!statusReason.value.trim()) {
    ElMessage.warning('请填写操作原因')
    return
  }
  try {
    await updateUserStatus(statusTarget.value.userId, {
      status: newStatus.value,
      reason: statusReason.value,
      bannedUntil: bannedUntil.value || null
    })
    ElMessage.success('操作成功')
    statusDialogVisible.value = false
    loadData()
    loadStatistics()
  } catch (error) {
    ElMessage.error('操作失败')
  }
}

const sendWarning = (row: any) => {
  warningTarget.value = row
  warningReason.value = ''
  warningDialogVisible.value = true
}

const confirmWarning = async () => {
  if (!warningReason.value.trim()) {
    ElMessage.warning('请填写警告内容')
    return
  }
  try {
    await sendUserWarning(warningTarget.value.userId, {
      reason: warningReason.value
    })
    ElMessage.success('警告已发送')
    warningDialogVisible.value = false
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
  loadStatistics()
})
</script>

<style scoped>
.user-manage {
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