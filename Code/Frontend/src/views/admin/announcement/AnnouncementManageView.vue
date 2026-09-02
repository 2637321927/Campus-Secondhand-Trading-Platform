<template>
  <div class="announcement-manage">
    <!-- 统计卡片 -->
    <el-row :gutter="20" class="stats-row">
      <el-col :span="8">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.total || 0 }}</div>
            <div class="stat-label">公告总数</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.published || 0 }}</div>
            <div class="stat-label">已发布</div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="8">
        <el-card>
          <div class="stat-item">
            <div class="stat-number">{{ statistics.draft || 0 }}</div>
            <div class="stat-label">草稿</div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 工具栏 -->
    <el-card class="toolbar-card">
      <div class="toolbar">
        <el-form :inline="true" :model="queryParams">
          <el-form-item label="关键词">
            <el-input
              v-model="queryParams.keyword"
              placeholder="标题/内容"
              clearable
              @keyup.enter="handleSearch"
            />
          </el-form-item>
          <el-form-item label="状态">
            <el-select v-model="queryParams.status" placeholder="全部状态" clearable>
              <el-option label="已发布" value="published" />
              <el-option label="草稿" value="draft" />
              <el-option label="已下架" value="archived" />
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="handleSearch">搜索</el-button>
            <el-button @click="resetSearch">重置</el-button>
            <el-button @click="loadData" :loading="loading">刷新</el-button>
          </el-form-item>
        </el-form>
        <el-button type="primary" @click="openCreateDialog">
          <el-icon><Plus /></el-icon>
          发布公告
        </el-button>
      </div>
    </el-card>

    <!-- 公告列表 -->
    <el-card class="table-card">
      <el-table :data="announcementList" v-loading="loading" border>
        <el-table-column prop="id" label="ID" width="70" />
        <el-table-column prop="title" label="标题" min-width="200">
          <template #default="{ row }">
            <div class="title-cell">
              <span class="title-text">{{ row.title }}</span>
              <el-tag v-if="row.isPinned" type="warning" size="small">置顶</el-tag>
            </div>
          </template>
        </el-table-column>
        <el-table-column prop="content" label="内容" min-width="200" show-overflow-tooltip />
        <el-table-column label="状态" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusType(row.status)">
              {{ getStatusText(row.status) }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="发布时间" width="160">
          <template #default="{ row }">
            {{ formatDate(row.publishTime) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="280" fixed="right">
          <template #default="{ row }">
            <el-button size="small" type="primary" @click="viewDetail(row)">
              查看
            </el-button>
            <el-button size="small" type="warning" @click="openEditDialog(row)">
              编辑
            </el-button>
            <el-button
              v-if="row.status === 'draft'"
              size="small"
              type="success"
              @click="handlePublish(row)"
            >
              发布
            </el-button>
            <el-button
              v-if="row.status === 'published'"
              size="small"
              type="info"
              @click="handleArchive(row)"
            >
              下架
            </el-button>
            <el-button size="small" type="danger" @click="handleDelete(row)">
              删除
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

    <!-- 创建/编辑公告对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle"
      width="700px"
      @close="resetForm"
    >
      <el-form
        ref="formRef"
        :model="formData"
        :rules="formRules"
        label-width="80px"
      >
        <el-form-item label="标题" prop="title">
          <el-input v-model="formData.title" placeholder="请输入公告标题" maxlength="100" show-word-limit />
        </el-form-item>
        <el-form-item label="内容" prop="content">
          <el-input
            v-model="formData.content"
            type="textarea"
            rows="8"
            placeholder="请输入公告内容"
            maxlength="2000"
            show-word-limit
          />
        </el-form-item>
        <el-form-item label="置顶">
          <el-switch v-model="formData.isPinned" />
          <span class="form-tip">置顶公告将显示在列表最前面</span>
        </el-form-item>
        <el-form-item label="状态">
          <el-radio-group v-model="formData.status">
            <el-radio label="draft">保存为草稿</el-radio>
            <el-radio label="published">立即发布</el-radio>
          </el-radio-group>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitLoading" @click="submitForm">
          {{ formData.id ? '保存修改' : '发布公告' }}
        </el-button>
      </template>
    </el-dialog>

    <!-- 公告详情对话框 -->
    <el-dialog v-model="detailDialogVisible" title="公告详情" width="600px">
      <div v-if="currentAnnouncement">
        <div class="detail-header">
          <h2>{{ currentAnnouncement.title }}</h2>
          <div class="detail-meta">
            <el-tag :type="getStatusType(currentAnnouncement.status)">
              {{ getStatusText(currentAnnouncement.status) }}
            </el-tag>
            <span v-if="currentAnnouncement.isPinned" class="pinned-badge">📌 置顶</span>
            <span class="detail-time">发布于：{{ formatDate(currentAnnouncement.publishTime) }}</span>
          </div>
        </div>
        <div class="detail-content">
          {{ currentAnnouncement.content }}
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
import { Plus } from '@element-plus/icons-vue'

const loading = ref(false)
const submitLoading = ref(false)
const announcementList = ref<any[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const dialogVisible = ref(false)
const detailDialogVisible = ref(false)
const dialogTitle = ref('')
const currentAnnouncement = ref<any>(null)
const formRef = ref()

const queryParams = reactive({
  keyword: '',
  status: undefined as string | undefined
})

const statistics = ref({
  total: 0,
  published: 0,
  draft: 0
})

// 状态映射
const statusMap: Record<string, { text: string; type: string }> = {
  draft: { text: '草稿', type: 'info' },
  published: { text: '已发布', type: 'success' },
  archived: { text: '已下架', type: 'danger' }
}

const getStatusText = (status: string) => statusMap[status]?.text || '未知'
const getStatusType = (status: string) => statusMap[status]?.type || 'info'

// 表单数据
const formData = reactive({
  id: undefined as number | undefined,
  title: '',
  content: '',
  isPinned: false,
  status: 'draft' as 'draft' | 'published'
})

const formRules = {
  title: [
    { required: true, message: '请输入公告标题', trigger: 'blur' },
    { min: 2, max: 100, message: '标题长度在 2 到 100 个字符', trigger: 'blur' }
  ],
  content: [
    { required: true, message: '请输入公告内容', trigger: 'blur' },
    { min: 10, max: 2000, message: '内容长度在 10 到 2000 个字符', trigger: 'blur' }
  ]
}

// 模拟数据
const mockAnnouncements = [
  {
    id: 1,
    title: '平台维护通知',
    content: '平台将于2026年9月10日凌晨2:00-4:00进行系统维护，届时将暂停服务，请提前做好准备。',
    status: 'published',
    isPinned: true,
    publishTime: new Date(Date.now() - 3600000).toISOString()
  },
  {
    id: 2,
    title: '新学期交易活动预告',
    content: '新学期即将开始，平台将于9月15日推出"开学季"二手交易活动，敬请期待！',
    status: 'draft',
    isPinned: false,
    publishTime: null
  }
]

const loadData = async () => {
  loading.value = true
  try {
    // TODO: 替换为真实 API
    // const res = await getAnnouncements({ ...queryParams, page: page.value, pageSize: pageSize.value })
    // announcementList.value = res.items
    // total.value = res.totalCount

    await new Promise(resolve => setTimeout(resolve, 500))
    announcementList.value = mockAnnouncements
    total.value = 2

    statistics.value = {
      total: 5,
      published: 3,
      draft: 2
    }
  } catch (error) {
    ElMessage.error('加载公告列表失败')
  } finally {
    loading.value = false
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

const resetForm = () => {
  formData.id = undefined
  formData.title = ''
  formData.content = ''
  formData.isPinned = false
  formData.status = 'draft'
}

const openCreateDialog = () => {
  resetForm()
  dialogTitle.value = '发布公告'
  dialogVisible.value = true
}

const openEditDialog = (row: any) => {
  formData.id = row.id
  formData.title = row.title
  formData.content = row.content
  formData.isPinned = row.isPinned || false
  formData.status = row.status === 'published' ? 'published' : 'draft'
  dialogTitle.value = '编辑公告'
  dialogVisible.value = true
}

const submitForm = async () => {
  const valid = await formRef.value?.validate().catch(() => false)
  if (!valid) return

  submitLoading.value = true
  try {
    if (formData.id) {
      // TODO: 调用更新公告 API
      // await updateAnnouncement(formData.id, formData)
      ElMessage.success('公告已更新')
    } else {
      // TODO: 调用创建公告 API
      // await createAnnouncement(formData)
      ElMessage.success('公告已发布')
    }
    dialogVisible.value = false
    loadData()
  } catch (error) {
    ElMessage.error('操作失败')
  } finally {
    submitLoading.value = false
  }
}

const viewDetail = (row: any) => {
  currentAnnouncement.value = row
  detailDialogVisible.value = true
}

const handlePublish = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要发布公告 "${row.title}" 吗？`, '发布公告', {
      type: 'success'
    })
    // TODO: 调用发布公告 API
    // await publishAnnouncement(row.id)
    ElMessage.success('公告已发布')
    loadData()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

const handleArchive = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要下架公告 "${row.title}" 吗？`, '下架公告', {
      type: 'warning'
    })
    // TODO: 调用下架公告 API
    // await archiveAnnouncement(row.id)
    ElMessage.success('公告已下架')
    loadData()
  } catch (error) {
    if (error !== 'cancel') {
      ElMessage.error('操作失败')
    }
  }
}

const handleDelete = async (row: any) => {
  try {
    await ElMessageBox.confirm(`确定要删除公告 "${row.title}" 吗？此操作不可恢复！`, '删除公告', {
      type: 'error'
    })
    // TODO: 调用删除公告 API
    // await deleteAnnouncement(row.id)
    ElMessage.success('公告已删除')
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
.announcement-manage {
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
.toolbar-card {
  margin-bottom: 20px;
}
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 10px;
}
.table-card {
  margin-top: 20px;
}
.pagination {
  margin-top: 20px;
  justify-content: flex-end;
}
.title-cell {
  display: flex;
  align-items: center;
  gap: 8px;
}
.title-text {
  font-weight: 500;
}
.form-tip {
  margin-left: 10px;
  color: #999;
  font-size: 12px;
}
.detail-header {
  border-bottom: 1px solid #e3e9e6;
  padding-bottom: 16px;
  margin-bottom: 16px;
}
.detail-header h2 {
  margin: 0 0 10px 0;
  font-size: 20px;
}
.detail-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  color: #999;
  font-size: 14px;
}
.pinned-badge {
  color: #e6a23c;
}
.detail-content {
  line-height: 1.8;
  white-space: pre-wrap;
  padding: 10px 0;
}
</style>