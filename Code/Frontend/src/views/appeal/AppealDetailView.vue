<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import {
    getAppealDetail,
    getAppealTimeline,
    appendAppealMessage,
    uploadAppealAttachment,
    cancelAppeal
} from '../../api/modules/appeal'
import type {
    AppealTimelineDto,
    WorkOrderDto
} from '../../types/api/appeal'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()

const appealId = computed(() => Number(route.params.appealId))

const loading = ref(false)
const errorMessage = ref('')
const appeal = ref<WorkOrderDto | null>(null)
const timeline = ref<AppealTimelineDto[]>([])
const operating = ref(false)

// 补充说明弹窗
const messageDialogVisible = ref(false)
const messageContent = ref('')

// 上传附件
const uploading = ref(false)

const statusTextMap: Record<string, string> = {
    waiting: '待处理',
    processing: '处理中',
    done: '已完成'
}

const statusTagType: Record<string, string> = {
    waiting: 'warning',
    processing: 'primary',
    done: 'success'
}

const actionTextMap: Record<string, string> = {
    accept: '已受理',
    reject: '已驳回',
    handle: '处理中',
    approve: '已通过',
    reply: '回复'
}

function formatDateTime(value: string | null): string {
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

async function loadAppeal(): Promise<void> {
    const id = appealId.value
    if (!id || Number.isNaN(id)) {
        errorMessage.value = '申诉ID无效'
        return
    }

    loading.value = true
    errorMessage.value = ''

    try {
        const response = await getAppealDetail(id)
        appeal.value = response.data

        // 加载时间线
        try {
            const timelineResponse = await getAppealTimeline(id)
            timeline.value = timelineResponse.data ?? []
        } catch {
            timeline.value = []
        }
    } catch (error) {
        appeal.value = null
        errorMessage.value = getApiErrorMessage(error, '申诉详情加载失败，请稍后重试')
        console.error('申诉详情加载失败：', error)
    } finally {
        loading.value = false
    }
}

function openMessageDialog(): void {
    messageContent.value = ''
    messageDialogVisible.value = true
}

async function handleAppendMessage(): Promise<void> {
    if (!appeal.value) return
    if (!messageContent.value.trim()) {
        ElMessage.warning('请输入补充说明')
        return
    }

    operating.value = true
    try {
        const response = await appendAppealMessage(appeal.value.id, {
            message: messageContent.value.trim()
        })
        appeal.value = response.data
        ElMessage.success('补充说明已提交')
        messageDialogVisible.value = false
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '补充说明提交失败'))
        console.error('补充说明提交失败：', error)
    } finally {
        operating.value = false
    }
}

async function handleUpload(file: File): Promise<void> {
    if (!appeal.value) return

    uploading.value = true
    try {
        const response = await uploadAppealAttachment(appeal.value.id, file)
        appeal.value = response.data
        ElMessage.success('附件上传成功')
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '附件上传失败'))
        console.error('附件上传失败：', error)
    } finally {
        uploading.value = false
    }
}

function onFileChange(event: Event): void {
    const target = event.target as HTMLInputElement
    const file = target.files?.[0]
    if (file) {
        void handleUpload(file)
    }
    target.value = ''
}

async function handleCancel(): Promise<void> {
    if (!appeal.value) return

    try {
        await ElMessageBox.confirm(
            '确定撤销此申诉吗？撤销后无法恢复。',
            '撤销申诉',
            { type: 'warning', confirmButtonText: '确定撤销', cancelButtonText: '取消' }
        )
    } catch {
        return
    }

    operating.value = true
    try {
        await cancelAppeal(appeal.value.id)
        ElMessage.success('申诉已撤销')
        router.push({ name: 'appeal-list' })
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '撤销申诉失败'))
        console.error('撤销申诉失败：', error)
    } finally {
        operating.value = false
    }
}

onMounted(() => {
    void loadAppeal()
})
</script>

<template>
    <main class="appeal-detail-page">
        <div class="appeal-detail-container">
            <!-- 返回按钮 -->
            <el-button text @click="router.back()">
                ← 返回
            </el-button>

            <!-- 加载状态 -->
            <section v-if="loading" class="detail-panel">
                <el-skeleton :rows="5" animated />
            </section>

            <!-- 错误状态 -->
            <el-result
                v-else-if="errorMessage"
                icon="error"
                title="申诉详情加载失败"
                :sub-title="errorMessage"
            >
                <template #extra>
                    <el-button type="primary" @click="loadAppeal">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 正常内容 -->
            <template v-else-if="appeal">
                <header class="page-header">
                    <p class="page-eyebrow">APPEAL DETAIL</p>
                    <div class="header-row">
                        <h1>申诉详情</h1>
                        <el-tag
                            :type="statusTagType[appeal.status] ?? 'info'"
                            effect="plain"
                            size="large"
                        >
                            {{ statusTextMap[appeal.status] ?? appeal.status }}
                        </el-tag>
                    </div>
                </header>

                <!-- 基本信息 -->
                <section class="detail-panel">
                    <h2 class="panel-title">基本信息</h2>
                    <div class="info-grid">
                        <div class="info-item">
                            <span class="info-label">申诉ID</span>
                            <span class="info-value">{{ appeal.id }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">申诉原因</span>
                            <span class="info-value">{{ appeal.reason }}</span>
                        </div>
                        <div class="info-item" v-if="appeal.appealAgainstId">
                            <span class="info-label">关联工单ID</span>
                            <span class="info-value">{{ appeal.appealAgainstId }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">创建时间</span>
                            <span class="info-value">{{ formatDateTime(appeal.createTime) }}</span>
                        </div>
                    </div>
                </section>

                <!-- 申诉说明 -->
                <section class="detail-panel" v-if="appeal.info">
                    <h2 class="panel-title">申诉说明</h2>
                    <p class="appeal-info-text">{{ appeal.info }}</p>
                </section>

                <!-- 处理结果 -->
                <section class="detail-panel" v-if="appeal.response">
                    <h2 class="panel-title">处理结果</h2>
                    <p class="appeal-info-text">{{ appeal.response }}</p>
                </section>

                <!-- 处理时间线 -->
                <section class="detail-panel" v-if="timeline.length > 0">
                    <h2 class="panel-title">处理流程</h2>
                    <el-timeline>
                        <el-timeline-item
                            v-for="item in timeline"
                            :key="item.timelineId"
                            :timestamp="formatDateTime(item.createTime)"
                        >
                            <p class="timeline-action">
                                {{ actionTextMap[item.action] ?? item.action }}
                            </p>
                            <p class="timeline-note" v-if="item.note">{{ item.note }}</p>
                        </el-timeline-item>
                    </el-timeline>
                </section>

                <!-- 操作 -->
                <section class="detail-panel" v-if="appeal.status !== 'done'">
                    <h2 class="panel-title">操作</h2>
                    <div class="action-buttons">
                        <el-button
                            type="primary"
                            :loading="operating"
                            @click="openMessageDialog"
                        >
                            补充说明
                        </el-button>
                        <label class="upload-label">
                            <input type="file" @change="onFileChange" :disabled="uploading" />
                            <el-button type="primary" :loading="uploading">
                                上传附件
                            </el-button>
                        </label>
                        <el-button
                            type="danger"
                            plain
                            :loading="operating"
                            @click="handleCancel"
                        >
                            撤销申诉
                        </el-button>
                    </div>
                </section>
            </template>
        </div>

        <!-- 补充说明弹窗 -->
        <el-dialog
            v-model="messageDialogVisible"
            title="补充说明"
            width="440px"
        >
            <el-input
                v-model="messageContent"
                type="textarea"
                :rows="5"
                placeholder="请输入补充说明"
                maxlength="500"
                show-word-limit
            />
            <template #footer>
                <el-button @click="messageDialogVisible = false">取消</el-button>
                <el-button type="primary" :loading="operating" @click="handleAppendMessage">
                    提交
                </el-button>
            </template>
        </el-dialog>
    </main>
</template>

<style scoped>
.appeal-detail-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.appeal-detail-container {
    width: 100%;
    max-width: 800px;
    margin: 0 auto;
}

.page-header {
    margin: 16px 0 24px;
}

.page-eyebrow {
    margin: 0 0 8px;
    color: #3e9b79;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 1.6px;
}

.header-row {
    display: flex;
    align-items: center;
    gap: 16px;
}

.page-header h1 {
    margin: 0;
    color: #1e2a26;
    font-size: 28px;
    line-height: 1.25;
}

.detail-panel {
    margin-bottom: 20px;
    padding: 24px 28px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
}

.panel-title {
    margin: 0 0 16px;
    color: #1e2a26;
    font-size: 18px;
}

.info-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 16px;
}

.info-item {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.info-label {
    color: #6c7a74;
    font-size: 13px;
}

.info-value {
    color: #1e2a26;
    font-size: 14px;
}

.appeal-info-text {
    margin: 0;
    color: #1e2a26;
    font-size: 14px;
    line-height: 1.7;
    white-space: pre-wrap;
}

.timeline-action {
    margin: 0;
    color: #1e2a26;
    font-size: 14px;
    font-weight: 600;
}

.timeline-note {
    margin: 4px 0 0;
    color: #6c7a74;
    font-size: 13px;
}

.action-buttons {
    display: flex;
    gap: 12px;
    align-items: center;
    flex-wrap: wrap;
}

.upload-label {
    position: relative;
    cursor: pointer;
    display: inline-flex;
}

.upload-label input[type="file"] {
    position: absolute;
    width: 0;
    height: 0;
    opacity: 0;
    overflow: hidden;
}

@media (max-width: 760px) {
    .info-grid {
        grid-template-columns: 1fr;
    }
}
</style>
