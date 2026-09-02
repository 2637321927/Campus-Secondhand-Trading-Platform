<script setup lang="ts">
import {
  computed,
  onMounted,
  ref
} from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { UploadRequestOptions } from 'element-plus'
import {
  getReportReasons,
  createReport,
  uploadReportAttachment,
  getProductReportInfo,
  getUserReportInfo
} from '../../api/modules/report'
import type {
  ReportReasonDto,
  ProductReportInfoDto,
  UserReportInfoDto
} from '../../types/api/report'
import { useAuthStore } from '../../stores/auth'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

/** 举报对象类型：product=商品，user=用户（由路由 query 决定） */
const targetType = ref<'product' | 'user'>(
  route.query.type === 'user' ? 'user' : 'product'
)

const targetId = ref<number | null>(
  route.query.targetId ? Number(route.query.targetId) : null
)

/** 被举报对象回显信息 */
const productInfo = ref<ProductReportInfoDto | null>(null)
const userInfo = ref<UserReportInfoDto | null>(null)
const loadingInfo = ref(false)
const infoError = ref('')

/** 表单 */
const reasons = ref<ReportReasonDto[]>([])
const selectedReason = ref('')
const infoText = ref('')
const submitting = ref(false)

/** 附件：先记下 File对象，提交成功后逐个上传 */
const pendingFiles = ref<File[]>([])
const uploadedReportId = ref<number | null>(null)
const uploadingCount = ref(0)

const canSubmit = computed(
  () =>
    targetId.value !== null &&
    targetId.value > 0 &&
    selectedReason.value !== ''
)

async function loadTargetInfo(): Promise<void> {
  if (!targetId.value) {
    infoError.value = '缺少举报对象，请从商品或用户页面进入'
    return
  }

  loadingInfo.value = true
  infoError.value = ''

  try {
    if (targetType.value === 'product') {
      const response = await getProductReportInfo(targetId.value)
      productInfo.value = response.data
    } else {
      const response = await getUserReportInfo(targetId.value)
      userInfo.value = response.data
    }
  } catch (error) {
    infoError.value = '举报对象不存在或已删除'

    console.error('举报对象信息加载失败：', error)
  } finally {
    loadingInfo.value = false
  }
}

async function loadReasons(): Promise<void> {
  try {
    const response = await getReportReasons()

    reasons.value = response.data ?? []
  } catch (error) {
    console.error('举报原因加载失败：', error)
  }
}

function handleFileChange(options: UploadRequestOptions): void {
  pendingFiles.value = [...pendingFiles.value, options.file]
}

function removePendingFile(index: number): void {
  pendingFiles.value = pendingFiles.value.filter(
    (_, i) => i !== index
  )
}

async function uploadAttachments(
  reportId: number
): Promise<void> {
  uploadingCount.value = pendingFiles.value.length

  for (const file of pendingFiles.value) {
    try {
      await uploadReportAttachment(reportId, file)
    } catch (error) {
      ElMessage.warning(
        `附件「${file.name}」上传失败，其余已提交`
      )

      console.error('举报附件上传失败：', error)
    } finally {
      uploadingCount.value -= 1
    }
  }
}

async function handleSubmit(): Promise<void> {
  if (!canSubmit.value || submitting.value) {
    return
  }

  if (!authStore.isLoggedIn) {
    ElMessage.warning('请先登录后再举报')

    void router.push({
      name: 'login',
      query: { redirect: route.fullPath }
    })

    return
  }

  submitting.value = true

  try {
    const response = await createReport({
      targetType: targetType.value,
      targetId: targetId.value!,
      reason: selectedReason.value,
      info: infoText.value.trim() || undefined,
      // 举报商品时把卖家作为被举报人；举报用户时 targetId 即被举报人
      accusedId:
        targetType.value === 'product'
          ? productInfo.value?.sellerId
          : targetId.value!,
      productId: targetType.value === 'product' ? targetId.value! : undefined
    })

    uploadedReportId.value = response.data.id

    ElMessage.success('举报已提交，我们会尽快处理')

    if (pendingFiles.value.length > 0) {
      await uploadAttachments(response.data.id)
    }

    void router.push({ name: 'user-reports' })
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '举报提交失败，请稍后重试')
    )

    console.error('举报提交失败：', error)
  } finally {
    submitting.value = false
  }
}

function goBack(): void {
  router.back()
}

onMounted(() => {
  void loadReasons()
  void loadTargetInfo()
})
</script>

<template>
  <main class="report-page">
    <div class="report-container">
      <header class="report-header">
        <button
          class="back-button"
          type="button"
          @click="goBack"
        >
          返回
        </button>

        <h1>发起举报</h1>

        <p class="report-subtitle">
          请如实描述问题，恶意举报可能影响你的账号信用。
        </p>
      </header>

      <!-- 被举报对象回显 -->
      <section
        v-loading="loadingInfo"
        class="target-section"
      >
        <h3 class="section-label">举报对象</h3>

        <el-result
          v-if="infoError"
          icon="warning"
          :title="infoError"
        />

        <div
          v-else-if="targetType === 'product' && productInfo"
          class="target-card"
        >
          <span class="target-tag">商品</span>

          <div class="target-body">
            <span class="target-title">{{ productInfo.name }}</span>

            <span class="target-meta">
              卖家 ID：{{ productInfo.sellerId }} · 状态：{{ productInfo.status }}
            </span>
          </div>
        </div>

        <div
          v-else-if="targetType === 'user' && userInfo"
          class="target-card"
        >
          <span class="target-tag">用户</span>

          <div class="target-body">
            <span class="target-title">{{ userInfo.userName }}</span>

            <span class="target-meta">
              用户 ID：{{ userInfo.userId }}{{ userInfo.profile ? ` · ${userInfo.profile}` : '' }}
            </span>
          </div>
        </div>
      </section>

      <!-- 举报表单 -->
      <section class="form-section">
        <div class="form-item">
          <h3 class="section-label">举报原因</h3>

          <el-select
            v-model="selectedReason"
            placeholder="请选择举报原因"
            class="reason-select"
          >
            <el-option
              v-for="reason in reasons"
              :key="reason.code"
              :label="reason.name"
              :value="reason.name"
            />
          </el-select>
        </div>

        <div class="form-item">
          <h3 class="section-label">补充说明（可选）</h3>

          <el-input
            v-model="infoText"
            type="textarea"
            :rows="4"
            maxlength="500"
            show-word-limit
            placeholder="描述具体问题，如交易经过、聊天记录要点等"
          />
        </div>

        <div class="form-item">
          <h3 class="section-label">上传附件（可选，提交后上传）</h3>

          <el-upload
            :show-file-list="false"
            :http-request="handleFileChange"
            accept="image/*,.pdf,.doc,.docx"
          >
            <el-button>添加附件</el-button>
          </el-upload>

          <ul
            v-if="pendingFiles.length > 0"
            class="file-list"
          >
            <li
              v-for="(file, index) in pendingFiles"
              :key="`${file.name}-${index}`"
              class="file-item"
            >
              <span class="file-name">{{ file.name }}</span>

              <button
                class="file-remove"
                type="button"
                @click="removePendingFile(index)"
              >
                移除
              </button>
            </li>
          </ul>
        </div>

        <el-button
          class="submit-button"
          type="primary"
          size="large"
          :loading="submitting || uploadingCount > 0"
          :disabled="!canSubmit"
          @click="handleSubmit"
        >
          {{ submitting ? '提交中…' : '提交举报' }}
        </el-button>
      </section>
    </div>
  </main>
</template>

<style scoped>
.report-page {
  min-height: calc(100vh - 72px);
  padding: 24px;
  background: #f5f7f6;
  color: #1e2a26;
}

.report-container {
  max-width: 720px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.report-header {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.report-header h1 {
  margin: 0;
  font-size: 22px;
}

.report-subtitle {
  margin: 0;
  color: #6c7a74;
  font-size: 13px;
}

.back-button {
  align-self: flex-start;
  padding: 6px 12px;
  color: #24735b;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.back-button:hover {
  background: #eef4f2;
}

.section-label {
  margin: 0 0 10px;
  font-size: 14px;
  font-weight: 600;
}

.target-section,
.form-section {
  padding: 20px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 14px;
}

.target-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px;
  background: #f7faf9;
  border: 1px solid #e3e9e6;
  border-radius: 10px;
}

.target-tag {
  flex-shrink: 0;
  padding: 4px 10px;
  color: #24735b;
  font-size: 12px;
  font-weight: 600;
  background: #e8f0ed;
  border-radius: 6px;
}

.target-body {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.target-title {
  font-size: 15px;
  font-weight: 600;
}

.target-meta {
  color: #6c7a74;
  font-size: 12px;
}

.form-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.reason-select {
  width: 100%;
}

.file-list {
  margin: 10px 0 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.file-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background: #f7faf9;
  border: 1px solid #e3e9e6;
  border-radius: 8px;
  font-size: 13px;
}

.file-remove {
  padding: 2px 8px;
  color: #b0685f;
  font-size: 12px;
  background: transparent;
  border: none;
  cursor: pointer;
}

.file-remove:hover {
  color: #d9544d;
}

.submit-button {
  align-self: flex-end;
  background: #24735b;
  border-color: #24735b;
}
</style>
