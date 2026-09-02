// 用户详情界面
<template>
  <div class="user-detail">
    <el-page-header @back="router.back()" content="返回用户列表" />

    <div v-loading="loading" class="detail-content">
      <!-- 基本信息 -->
      <el-card class="info-card">
        <template #header>
          <div class="card-header">
            <span>用户信息</span>
          </div>
        </template>
        <el-descriptions :column="3" border>
          <el-descriptions-item label="用户ID">{{ userInfo.userId }}</el-descriptions-item>
          <el-descriptions-item label="昵称">{{ userInfo.userName }}</el-descriptions-item>
          <el-descriptions-item label="邮箱">{{ userInfo.email || '-' }}</el-descriptions-item>
          <el-descriptions-item label="手机号">{{ userInfo.phoneNumber || '-' }}</el-descriptions-item>
          <el-descriptions-item label="用户类型">
            <el-tag :type="userInfo.userType === 1 ? 'danger' : ''">
              {{ userInfo.userType === 1 ? '管理员' : '普通用户' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="账户状态">
            <el-tag :type="getStatusType(userInfo.accountStatus)">
              {{ getStatusText(userInfo.accountStatus) }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="信誉分">
            <span :style="{ color: userInfo.credit >= 80 ? '#67c23a' : '#f56c6c' }">
              {{ userInfo.credit }}
            </span>
          </el-descriptions-item>
          <el-descriptions-item label="注册时间">{{ formatDate(userInfo.registerTime) }}</el-descriptions-item>
          <el-descriptions-item label="封禁截止">{{ userInfo.bannedUntil ? formatDate(userInfo.bannedUntil) : '无' }}</el-descriptions-item>
        </el-descriptions>
      </el-card>

      <!-- 统计概览 -->
      <el-row :gutter="20" class="stats-row">
        <el-col :span="6">
          <el-card>
            <div class="stat-item">
              <div class="stat-number">{{ userInfo.productCount || 0 }}</div>
              <div class="stat-label">发布商品</div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card>
            <div class="stat-item">
              <div class="stat-number">{{ userInfo.orderCount || 0 }}</div>
              <div class="stat-label">相关订单</div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card>
            <div class="stat-item">
              <div class="stat-number">{{ userInfo.warningCount || 0 }}</div>
              <div class="stat-label">警告次数</div>
            </div>
          </el-card>
        </el-col>
        <el-col :span="6">
          <el-card>
            <div class="stat-item">
              <div class="stat-number">{{ userInfo.violationCount || 0 }}</div>
              <div class="stat-label">违规次数</div>
            </div>
          </el-card>
        </el-col>
      </el-row>

      <!-- 操作按钮 -->
      <el-card class="action-card">
        <template #header>
          <span>操作</span>
        </template>
        <el-space>
          <el-button
            :type="userInfo.accountStatus === 0 ? 'danger' : 'success'"
            @click="toggleStatus"
          >
            {{ userInfo.accountStatus === 0 ? '封禁用户' : '解封用户' }}
          </el-button>
          <el-button type="warning" @click="openWarningDialog">发送警告</el-button>
          <el-button type="primary" @click="viewUserProducts">查看发布的商品</el-button>
        </el-space>
      </el-card>

      <!-- 操作历史 -->
      <el-card class="history-card" v-if="reputationData">
        <template #header>
          <span>信誉与违规记录</span>
        </template>
        <div v-if="reputationData.recentWarnings?.length">
          <el-table :data="reputationData.recentWarnings" border>
            <el-table-column prop="reason" label="原因" />
            <el-table-column prop="createTime" label="时间" width="180">
              <template #default="{ row }">{{ formatDate(row.createTime) }}</template>
            </el-table-column>
          </el-table>
        </div>
        <el-empty v-else description="暂无违规记录" />
      </el-card>
    </div>

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
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
  getAdminUserDetail,
  getUserReputation,
  updateUserStatus,
  sendUserWarning
} from '../../../api/modules/admin' 

const route = useRoute()
const router = useRouter()
const loading = ref(false)
const userId = Number(route.params.userId)

const userInfo = ref<any>({
  userId: 0,
  userName: '',
  email: '',
  phoneNumber: '',
  userType: 0,
  accountStatus: 0,
  credit: 0,
  registerTime: '',
  bannedUntil: null,
  productCount: 0,
  orderCount: 0,
  warningCount: 0,
  violationCount: 0
})

const reputationData = ref<any>(null)

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
const newStatus = ref<number>(0)
const bannedUntil = ref<string | null>(null)

// 警告
const warningDialogVisible = ref(false)
const warningReason = ref('')

const loadData = async () => {
  loading.value = true
  try {
    const [user, reputation] = await Promise.all([
      getAdminUserDetail(userId),
      getUserReputation(userId).catch(() => null)
    ])
    userInfo.value = user
    reputationData.value = reputation
  } catch (error) {
    ElMessage.error('加载用户信息失败')
  } finally {
    loading.value = false
  }
}

const toggleStatus = () => {
  if (userInfo.value.accountStatus === 0) {
    statusDialogTitle.value = `封禁用户 "${userInfo.value.userName}"`
    newStatus.value = 3
  } else {
    statusDialogTitle.value = `解封用户 "${userInfo.value.userName}"`
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
    await updateUserStatus(userId, {
      status: newStatus.value,
      reason: statusReason.value,
      bannedUntil: bannedUntil.value || null
    })
    ElMessage.success('操作成功')
    statusDialogVisible.value = false
    loadData()
  } catch (error) {
    ElMessage.error('操作失败')
  }
}

const openWarningDialog = () => {
  warningReason.value = ''
  warningDialogVisible.value = true
}

const confirmWarning = async () => {
  if (!warningReason.value.trim()) {
    ElMessage.warning('请填写警告内容')
    return
  }
  try {
    await sendUserWarning(userId, { reason: warningReason.value })
    ElMessage.success('警告已发送')
    warningDialogVisible.value = false
    loadData()
  } catch (error) {
    ElMessage.error('发送失败')
  }
}

const viewUserProducts = () => {
  router.push(`/admin/products?sellerId=${userId}`)
}

const formatDate = (date: string) => {
  if (!date) return '-'
  return new Date(date).toLocaleString('zh-CN')
}

onMounted(() => {
  if (userId) {
    loadData()
  }
})
</script>

<style scoped>
.user-detail {
  padding: 20px;
}
.detail-content {
  margin-top: 20px;
}
.info-card {
  margin-bottom: 20px;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
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
.action-card {
  margin-bottom: 20px;
}
.history-card {
  margin-bottom: 20px;
}
</style>