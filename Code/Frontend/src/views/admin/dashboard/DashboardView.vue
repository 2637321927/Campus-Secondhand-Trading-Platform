// 数据概览
<template>
  <div class="dashboard">
    <el-row :gutter="20" class="stats-row">
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ productStats.totalProducts || 0 }}</div>
            <div class="stat-label">商品总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ productStats.pendingReviewCount || 0 }}</div>
            <div class="stat-label">待审核商品</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ userStats.totalUsers || 0 }}</div>
            <div class="stat-label">用户总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="6">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ moderationTasks.totalPending || 0 }}</div>
            <div class="stat-label">待处理工单</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :span="12">
        <el-card>
          <template #header>
            <span>商品状态分布</span>
          </template>
          <div class="chart-placeholder">
            <div class="chart-bars">
              <div class="bar-item">
                <span>在售</span>
                <div class="bar-track">
                  <div class="bar-fill" :style="{ width: getPercent(productStats.availableCount, productStats.totalProducts) }"></div>
                </div>
                <span>{{ productStats.availableCount || 0 }}</span>
              </div>
              <div class="bar-item">
                <span>待审核</span>
                <div class="bar-track">
                  <div class="bar-fill warning" :style="{ width: getPercent(productStats.pendingReviewCount, productStats.totalProducts) }"></div>
                </div>
                <span>{{ productStats.pendingReviewCount || 0 }}</span>
              </div>
              <div class="bar-item">
                <span>已驳回</span>
                <div class="bar-track">
                  <div class="bar-fill danger" :style="{ width: getPercent(productStats.rejectedCount, productStats.totalProducts) }"></div>
                </div>
                <span>{{ productStats.rejectedCount || 0 }}</span>
              </div>
              <div class="bar-item">
                <span>已下架</span>
                <div class="bar-track">
                  <div class="bar-fill info" :style="{ width: getPercent(productStats.removedCount, productStats.totalProducts) }"></div>
                </div>
                <span>{{ productStats.removedCount || 0 }}</span>
              </div>
              <div class="bar-item">
                <span>已售</span>
                <div class="bar-track">
                  <div class="bar-fill success" :style="{ width: getPercent(productStats.soldCount, productStats.totalProducts) }"></div>
                </div>
                <span>{{ productStats.soldCount || 0 }}</span>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>
            <span>用户状态分布</span>
          </template>
          <div class="chart-placeholder">
            <div class="chart-bars">
              <div class="bar-item">
                <span>正常</span>
                <div class="bar-track">
                  <div class="bar-fill success" :style="{ width: getPercent(userStats.normalUsers, userStats.totalUsers) }"></div>
                </div>
                <span>{{ userStats.normalUsers || 0 }}</span>
              </div>
              <div class="bar-item">
                <span>禁言</span>
                <div class="bar-track">
                  <div class="bar-fill warning" :style="{ width: getPercent(userStats.mutedUsers, userStats.totalUsers) }"></div>
                </div>
                <span>{{ userStats.mutedUsers || 0 }}</span>
              </div>
              <div class="bar-item">
                <span>限制发布</span>
                <div class="bar-track">
                  <div class="bar-fill warning" :style="{ width: getPercent(userStats.publishRestrictedUsers, userStats.totalUsers) }"></div>
                </div>
                <span>{{ userStats.publishRestrictedUsers || 0 }}</span>
              </div>
              <div class="bar-item">
                <span>封禁</span>
                <div class="bar-track">
                  <div class="bar-fill danger" :style="{ width: getPercent(userStats.bannedUsers, userStats.totalUsers) }"></div>
                </div>
                <span>{{ userStats.bannedUsers || 0 }}</span>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 待处理任务 -->
    <el-card class="task-card" style="margin-top: 20px">
      <template #header>
        <span>待处理任务</span>
      </template>
      <el-empty v-if="!moderationTasks.recentTasks?.length" description="暂无待处理任务" />
      <el-table v-else :data="moderationTasks.recentTasks" border>
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="type" label="类型" width="100">
          <template #default="{ row }">
            <el-tag :type="row.type === 'report' ? 'danger' : 'warning'">
              {{ row.type === 'report' ? '举报' : '申诉' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="title" label="标题" min-width="200" />
        <el-table-column prop="status" label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="row.status === 'waiting' ? 'warning' : 'primary'">
              {{ row.status === 'waiting' ? '待处理' : '处理中' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="提交时间" width="160">
          <template #default="{ row }">{{ formatDate(row.createTime) }}</template>
        </el-table-column>
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="goToDetail(row)">
              处理
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import {
  getProductStatistics,
  getUserStatistics,
  getModerationTasks
}  from '../../../api/modules/admin'

const router = useRouter()

const productStats = ref({
  totalProducts: 0,
  availableCount: 0,
  soldCount: 0,
  removedCount: 0,
  pendingReviewCount: 0,
  rejectedCount: 0,
  newProductsToday: 0
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

const moderationTasks = ref({
  totalPending: 0,
  waitingCount: 0,
  processingCount: 0,
  reportCount: 0,
  appealCount: 0,
  recentTasks: []
})

const getPercent = (value: number, total: number) => {
  if (!total) return '0%'
  return `${(value / total * 100).toFixed(1)}%`
}

const loadData = async () => {
  try {
    const [products, users, tasks] = await Promise.all([
      getProductStatistics(),
      getUserStatistics(),
      getModerationTasks()
    ])
    productStats.value = products
    userStats.value = users
    moderationTasks.value = tasks
  } catch (error) {
    console.error('加载仪表盘数据失败', error)
  }
}

const goToDetail = (row: any) => {
  if (row.type === 'report') {
    router.push(`/admin/reports/${row.id}`)
  } else {
    router.push(`/admin/appeals/${row.id}`)
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
.dashboard {
  padding: 20px;
}
.stats-row {
  margin-bottom: 20px;
}
.stat-item {
  text-align: center;
}
.stat-number {
  font-size: 32px;
  font-weight: bold;
  color: #24735b;
}
.stat-label {
  color: #666;
  margin-top: 5px;
}
.chart-placeholder {
  padding: 20px 0;
}
.chart-bars {
  display: flex;
  flex-direction: column;
  gap: 12px;
}
.bar-item {
  display: flex;
  align-items: center;
  gap: 12px;
}
.bar-item > span:first-child {
  width: 60px;
  color: #666;
  font-size: 14px;
}
.bar-item > span:last-child {
  width: 40px;
  text-align: right;
  font-weight: bold;
}
.bar-track {
  flex: 1;
  height: 20px;
  background: #f0f0f0;
  border-radius: 10px;
  overflow: hidden;
}
.bar-fill {
  height: 100%;
  border-radius: 10px;
  background: #24735b;
  transition: width 0.5s ease;
}
.bar-fill.warning {
  background: #e6a23c;
}
.bar-fill.danger {
  background: #f56c6c;
}
.bar-fill.info {
  background: #909399;
}
.bar-fill.success {
  background: #67c23a;
}
.task-card {
  margin-top: 20px;
}
</style>