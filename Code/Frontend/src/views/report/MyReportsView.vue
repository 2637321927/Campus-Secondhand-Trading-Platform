<script setup lang="ts">
import {
  onMounted,
  ref
} from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { UploadRequestOptions } from 'element-plus'
import {
  getMyReports,
  getMyAppeals,
  cancelReport,
  cancelAppeal,
  appendAppealMessage,
  getReportReasons
} from '../../api/modules/report'
import type {
  WorkOrderDto,
  WorkOrderStatus,
  ReportReasonDto
} from '../../types/api/report'
import { getApiErrorMessage } from '../../utils/error'
import request from '../../api/http'

const router = useRouter()

const activeTab = ref('report')

// ===== 举报 =====
const reports = ref<WorkOrderDto[]>([])
const loadingReports = ref(false)
const reportsError = ref('')

// ===== 申诉 =====
const appeals = ref<WorkOrderDto[]>([])
const loadingAppeals = ref(false)
const appealsError = ref('')

// ===== 操作状态 =====
const cancellingIds = ref<number[]>([])
const appendingId = ref<number | null>(null)
const uploadTargetId = ref<number | null>(null)

/** 展开的工单（查看处理结果/补充说明） */
const expandedIds = ref<number[]>([])

const reasons = ref<ReportReasonDto[]>([])

function reasonName(value: string): string {
  return (
    reasons.value.find((r) => r.name === value)?.name ?? value
  )
}

function statusText(status: WorkOrderStatus): string {
  if (status === 'waiting') return '待处理'
  if (status === 'processing') return '处理中'
  return '已完成'
}

function statusType(
  status: WorkOrderStatus
): 'warning' | 'primary' | 'success' {
  if (status === 'waiting') return 'warning'
  if (status === 'processing') return 'primary'
  return 'success'
}

function formatTime(value?: string): string {
  if (!value) return ''

  const date = new Date(value)

  if (Number.isNaN(date.getTime())) return ''

  return date.toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit'
  })
}

/** Info 字段里的附件标记行："[附件:fileId:fileName]" */
function parseInfoParts(info: string | null): {
  text: string
  attachments: Array<{ fileId: number; fileName: string }>
} {
  if (!info) return { text: '', attachments: [] }

  const attachments: Array<{ fileId: number; fileName: string }> = []
  const lines = info
    .split('\n')
    .filter((line) => line.trim() !== '')

  const kept = lines.filter((line) => {
    const match = line.trim().match(/^\[附件:(\d+):(.+)\]$/)

    if (match) {
      attachments.push({
        fileId: Number(match[1]),
        fileName: match[2]
      })
      return false
    }

    return true
  })

  return { text: kept.join('\n'), attachments }
}

function isCancelling(id: number): boolean {
  return cancellingIds.value.includes(id)
}

function isExpanded(id: number): boolean {
  return expandedIds.value.includes(id)
}

function toggleExpand(id: number): void {
  expandedIds.value = isExpanded(id)
    ? expandedIds.value.filter((x) => x !== id)
    : [...expandedIds.value, id]
}

async function loadReports(): Promise<void> {
  loadingReports.value = true
  reportsError.value = ''

  try {
    const response = await getMyReports()

    reports.value = response.data ?? []
  } catch (error) {
    reportsError.value = '举报列表加载失败，请稍后重试'

    console.error('举报列表加载失败：', error)
  } finally {
    loadingReports.value = false
  }
}

async function loadAppeals(): Promise<void> {
  loadingAppeals.value = true
  appealsError.value = ''

  try {
    const response = await getMyAppeals()

    appeals.value = response.data ?? []
  } catch (error) {
    appealsError.value = '申诉列表加载失败，请稍后重试'

    console.error('申诉列表加载失败：', error)
  } finally {
    loadingAppeals.value = false
  }
}

async function handleCancel(order: WorkOrderDto): Promise<void> {
  const label = order.type === 1 ? '举报' : '申诉'

  try {
    await ElMessageBox.confirm(
      `确定撤销这条${label}吗？撤销后不可恢复。`,
      `撤销${label}`,
      {
        type: 'warning',
        confirmButtonText: '撤销',
        cancelButtonText: '取消'
      }
    )
  } catch {
    return
  }

  cancellingIds.value = [...cancellingIds.value, order.id]

  try {
    if (order.type === 1) {
      await cancelReport(order.id)
    } else {
      await cancelAppeal(order.id)
    }

    ElMessage.success(`${label}已撤销`)

    if (order.type === 1) {
      await loadReports()
    } else {
      await loadAppeals()
    }
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, `撤销${label}失败，请稍后重试`)
    )

    console.error(`撤销${label}失败：`, error)
  } finally {
    cancellingIds.value = cancellingIds.value.filter(
      (id) => id !== order.id
    )
  }
}

async function handleAppendMessage(
  order: WorkOrderDto,
  message: string
): Promise<void> {
  if (!message.trim()) return

  appendingId.value = order.id

  try {
    const response = await appendAppealMessage(order.id, message.trim())

    const index = appeals.value.findIndex((a) => a.id === order.id)

    if (index !== -1) {
      appeals.value[index] = response.data
    }

    ElMessage.success('补充说明已提交')
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '补充说明提交失败，请稍后重试')
    )

    console.error('补充说明提交失败：', error)
  } finally {
    appendingId.value = null
  }
}

/** 补充说明弹窗状态 */
const appendDialogVisible = ref(false)
const appendText = ref('')
const appendTarget = ref<WorkOrderDto | null>(null)

function openAppendDialog(order: WorkOrderDto): void {
  appendTarget.value = order
  appendText.value = ''
  appendDialogVisible.value = true
}

function submitAppend(): void {
  if (!appendTarget.value || !appendText.value.trim()) return

  void handleAppendMessage(appendTarget.value, appendText.value)

  appendDialogVisible.value = false
}

async function handleUploadAttachment(
  options: UploadRequestOptions
): Promise<void> {
  if (!uploadTargetId.value) return

  const appealId = uploadTargetId.value

  try {
    const formData = new FormData()
    formData.append('file', options.file)

    const response = await request.post<WorkOrderDto>(
      `/api/appeals/${appealId}/attachments`,
      formData
    )

    const index = appeals.value.findIndex((a) => a.id === appealId)

    if (index !== -1) {
      appeals.value[index] = response.data
    }

    ElMessage.success('附件已上传')
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '附件上传失败，请稍后重试')
    )

    console.error('申诉附件上传失败：', error)
  }
}

/** 时间线接口返回体未定型，先展示条数并在控制台打印 */
async function loadTimeline(order: WorkOrderDto): Promise<void> {
  if (order.type !== 2) return

  toggleExpand(order.id)

  if (isExpanded(order.id)) {
    try {
      const response = await request.get<unknown[]>(
        `/api/appeals/${order.id}/timeline`
      )

      if (Array.isArray(response.data) && response.data.length > 0) {
        console.info(`申诉 ${order.id} 时间线：`, response.data)
      }
    } catch (error) {
      console.warn('申诉时间线加载失败：', error)
    }
  }
}

function goCreateAppeal(): void {
  void router.push({ name: 'appeal-create' })
}

function goCreateReport(): void {
  void router.push({ name: 'report-create' })
}

onMounted(() => {
  void loadReports()
  void loadAppeals()
  void getReportReasons()
    .then((response) => {
      reasons.value = response.data ?? []
    })
    .catch((error) => {
      console.warn('举报原因加载失败：', error)
    })
})
</script>

<template>
  <main class="orders-page">
    <div class="orders-container">
      <header class="orders-header">
        <h1>我的举报与申诉</h1>

        <p class="orders-subtitle">
          查看你发起的举报和申诉的处理进度，可补充说明或撤销未处理的工单。
        </p>
      </header>

      <el-tabs
        v-model="activeTab"
        class="orders-tabs"
      >
        <!-- 举报 tab -->
        <el-tab-pane
          label="我的举报"
          name="report"
        >
          <el-result
            v-if="reportsError"
            icon="error"
            title="举报列表加载失败"
            :sub-title="reportsError"
          >
            <template #extra>
              <el-button
                type="primary"
                @click="loadReports"
              >
                重新加载
              </el-button>
            </template>
          </el-result>

          <el-empty
            v-else-if="
              !loadingReports && reports.length === 0
            "
            description="暂无举报记录"
          >
            <el-button
              type="primary"
              @click="goCreateReport"
            >
              发起举报
            </el-button>
          </el-empty>

          <ul
            v-else
            v-loading="loadingReports"
            class="order-list"
          >
            <li
              v-for="order in reports"
              :key="order.id"
              class="order-card"
            >
              <div class="order-top">
                <el-tag :type="statusType(order.status)">
                  {{ statusText(order.status) }}
                </el-tag>

                <span class="order-reason">
                  {{ reasonName(order.reason) }}
                </span>

                <span class="order-target">
                  举报对象：
                  {{ order.targetType === 'product' ? `商品 #${order.targetId}` : `用户 #${order.targetId}` }}
                </span>

                <span class="order-time">
                  {{ formatTime(order.createTime) }}
                </span>
              </div>

              <p
                v-if="parseInfoParts(order.info).text"
                class="order-info"
              >
                {{ parseInfoParts(order.info).text }}
              </p>

              <!-- 已处理：展示结果与回复 -->
              <div
                v-if="order.status === 'done'"
                class="order-result"
              >
                <p v-if="order.result">
                  <strong>处理结果：</strong>{{ order.result }}
                </p>

                <p v-if="order.response">
                  <strong>平台回复：</strong>{{ order.response }}
                </p>
              </div>

              <div class="order-actions">
                <el-button
                  v-if="order.status !== 'done'"
                  type="danger"
                  link
                  :loading="isCancelling(order.id)"
                  @click="handleCancel(order)"
                >
                  撤销举报
                </el-button>
              </div>
            </li>
          </ul>
        </el-tab-pane>

        <!-- 申诉 tab -->
        <el-tab-pane
          label="我的申诉"
          name="appeal"
        >
          <div class="appeal-hint">
            <span>
              对商品下架、账号受限或举报处理结果有异议？可发起申诉。
            </span>

            <el-button
              size="small"
              type="primary"
              @click="goCreateAppeal"
            >
              发起申诉
            </el-button>
          </div>

          <el-result
            v-if="appealsError"
            icon="error"
            title="申诉列表加载失败"
            :sub-title="appealsError"
          >
            <template #extra>
              <el-button
                type="primary"
                @click="loadAppeals"
              >
                重新加载
              </el-button>
            </template>
          </el-result>

          <el-empty
            v-else-if="
              !loadingAppeals && appeals.length === 0
            "
            description="暂无申诉记录"
          />

          <ul
            v-else
            v-loading="loadingAppeals"
            class="order-list"
          >
            <li
              v-for="order in appeals"
              :key="order.id"
              class="order-card"
            >
              <div class="order-top">
                <el-tag :type="statusType(order.status)">
                  {{ statusText(order.status) }}
                </el-tag>

                <span class="order-reason">
                  {{ order.reason }}
                </span>

                <span
                  v-if="order.appealAgainstId"
                  class="order-target"
                >
                  针对工单 #{{ order.appealAgainstId }}
                </span>

                <span class="order-time">
                  {{ formatTime(order.createTime) }}
                </span>
              </div>

              <p
                v-if="parseInfoParts(order.info).text"
                class="order-info"
              >
                {{ parseInfoParts(order.info).text }}
              </p>

              <!-- 附件列表（从 Info 里的 [附件:fileId:fileName] 解析） -->
              <div
                v-if="parseInfoParts(order.info).attachments.length > 0"
                class="order-attachments"
              >
                <span class="attachment-label">附件：</span>

                <span
                  v-for="file in parseInfoParts(order.info).attachments"
                  :key="file.fileId"
                  class="attachment-item"
                >
                  {{ file.fileName }}（文件ID {{ file.fileId }}）
                </span>
              </div>

              <!-- 已处理：展示结果与回复 -->
              <div
                v-if="order.status === 'done'"
                class="order-result"
              >
                <p v-if="order.result">
                  <strong>处理结果：</strong>{{ order.result }}
                </p>

                <p v-if="order.response">
                  <strong>平台回复：</strong>{{ order.response }}
                </p>
              </div>

              <div class="order-actions">
                <el-button
                  link
                  type="primary"
                  @click="loadTimeline(order)"
                >
                  {{ isExpanded(order.id) ? '收起' : '处理时间线' }}
                </el-button>

                <el-button
                  v-if="order.status !== 'done'"
                  link
                  type="primary"
                  @click="openAppendDialog(order)"
                >
                  补充说明
                </el-button>

                <el-upload
                  v-if="order.status !== 'done'"
                  :show-file-list="false"
                  :http-request="handleUploadAttachment"
                  @click.capture="uploadTargetId = order.id"
                >
                  <el-button
                    link
                    type="primary"
                  >
                    上传附件
                  </el-button>
                </el-upload>

                <el-popconfirm
                  v-if="order.status !== 'done'"
                  title="确定撤销这条申诉吗？"
                  confirm-button-text="撤销"
                  cancel-button-text="取消"
                  @confirm="handleCancel(order)"
                >
                  <template #reference>
                    <el-button
                      type="danger"
                      link
                      :loading="isCancelling(order.id)"
                    >
                      撤销申诉
                    </el-button>
                  </template>
                </el-popconfirm>
              </div>
            </li>
          </ul>
        </el-tab-pane>
      </el-tabs>

      <!-- 补充说明弹窗 -->
      <el-dialog
        v-model="appendDialogVisible"
        title="补充说明"
        width="480px"
      >
        <el-input
          v-model="appendText"
          type="textarea"
          :rows="4"
          maxlength="500"
          show-word-limit
          placeholder="补充与该申诉相关的说明"
        />

        <template #footer>
          <el-button @click="appendDialogVisible = false">
            取消
          </el-button>

          <el-button
            type="primary"
            :loading="appendingId !== null"
            @click="submitAppend"
          >
            提交
          </el-button>
        </template>
      </el-dialog>
    </div>
  </main>
</template>

<style scoped>
.orders-page {
  min-height: calc(100vh - 72px);
  padding: 24px;
  background: #f5f7f6;
  color: #1e2a26;
}

.orders-container {
  max-width: 860px;
  margin: 0 auto;
}

.orders-header h1 {
  margin: 0 0 6px;
  font-size: 22px;
}

.orders-subtitle {
  margin: 0 0 16px;
  color: #6c7a74;
  font-size: 13px;
}

.order-list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 14px;
  min-height: 120px;
}

.order-card {
  padding: 16px 18px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 12px;
}

.order-top {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
}

.order-reason {
  font-size: 15px;
  font-weight: 600;
}

.order-target {
  color: #6c7a74;
  font-size: 12px;
}

.order-time {
  margin-left: auto;
  color: #93a39e;
  font-size: 12px;
}

.order-info {
  margin: 10px 0 0;
  color: #43534e;
  font-size: 13px;
  line-height: 1.6;
  white-space: pre-wrap;
  word-break: break-word;
}

.order-attachments {
  display: flex;
  margin-top: 8px;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  font-size: 12px;
}

.attachment-label {
  color: #6c7a74;
}

.attachment-item {
  padding: 3px 8px;
  background: #f2f5f4;
  border-radius: 6px;
}

.order-result {
  margin-top: 10px;
  padding: 10px 14px;
  background: #f7faf9;
  border-left: 3px solid #24735b;
  border-radius: 6px;
  font-size: 13px;
  line-height: 1.7;
}

.order-result p {
  margin: 0;
}

.order-actions {
  display: flex;
  margin-top: 10px;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
}

.appeal-hint {
  display: flex;
  margin-bottom: 14px;
  padding: 12px 16px;
  align-items: center;
  justify-content: space-between;
  background: #eef4f2;
  border-radius: 10px;
  font-size: 13px;
}
</style>
