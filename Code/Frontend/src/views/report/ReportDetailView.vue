<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getReportDetail, cancelReport, uploadReportAttachment } from '../../api/modules/report'
import type { WorkOrderDto } from '../../types/api/report'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()

const reportId = computed(() => Number(route.params.reportId))

const loading = ref(false)
const errorMessage = ref('')
const report = ref<WorkOrderDto | null>(null)
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

const reasonTextMap: Record<string, string> = {
    fraud: '欺诈或虚假信息',
    illegal: '违禁或违法内容',
    spam: '骚扰或垃圾信息',
    other: '其他'
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

async function loadReport(): Promise<void> {
    loading.value = true
    errorMessage.value = ''

    try {
        const response = await getReportDetail(reportId.value)
        report.value = response.data
    } catch (error) {
        report.value = null
        errorMessage.value = getApiErrorMessage(error, '举报详情加载失败，请稍后重试')
        console.error('举报详情加载失败：', error)
    } finally {
        loading.value = false
    }
}

async function handleCancel(): Promise<void> {
    if (!report.value) return

    try {
        await ElMessageBox.confirm(
            '确定撤销此举报吗？撤销后无法恢复。',
            '撤销举报',
            { type: 'warning', confirmButtonText: '确定撤销', cancelButtonText: '取消' }
        )
    } catch {
        return
    }

    try {
        await cancelReport(report.value.id)
        ElMessage.success('举报已撤销')
        router.push({ name: 'report-list' })
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '撤销举报失败'))
        console.error('撤销举报失败：', error)
    }
}

async function handleUpload(file: File): Promise<void> {
    if (!report.value) return

    uploading.value = true
    try {
        const response = await uploadReportAttachment(report.value.id, file)
        report.value = response.data
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

onMounted(() => {
    void loadReport()
})
</script>

<template>
    <main class="report-detail-page">
        <div class="report-detail-container">
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
                title="举报详情加载失败"
                :sub-title="errorMessage"
            >
                <template #extra>
                    <el-button type="primary" @click="loadReport">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 正常内容 -->
            <template v-else-if="report">
                <header class="page-header">
                    <p class="page-eyebrow">REPORT DETAIL</p>
                    <div class="header-row">
                        <h1>举报详情</h1>
                        <el-tag
                            :type="statusTagType[report.status] ?? 'info'"
                            effect="plain"
                            size="large"
                        >
                            {{ statusTextMap[report.status] ?? report.status }}
                        </el-tag>
                    </div>
                </header>

                <!-- 基本信息 -->
                <section class="detail-panel">
                    <h2 class="panel-title">基本信息</h2>
                    <div class="info-grid">
                        <div class="info-item">
                            <span class="info-label">举报ID</span>
                            <span class="info-value">{{ report.id }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">举报原因</span>
                            <span class="info-value">{{ reasonTextMap[report.reason] ?? report.reason }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">对象类型</span>
                            <span class="info-value">{{ report.targetType ?? '未知' }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">对象ID</span>
                            <span class="info-value">{{ report.targetId ?? '无' }}</span>
                        </div>
                        <div class="info-item">
                            <span class="info-label">创建时间</span>
                            <span class="info-value">{{ formatDateTime(report.createTime) }}</span>
                        </div>
                    </div>
                </section>

                <!-- 详细说明 -->
                <section class="detail-panel" v-if="report.info">
                    <h2 class="panel-title">详细说明</h2>
                    <p class="report-info-text">{{ report.info }}</p>
                </section>

                <!-- 处理结果 -->
                <section class="detail-panel" v-if="report.response">
                    <h2 class="panel-title">处理结果</h2>
                    <p class="report-info-text">{{ report.response }}</p>
                </section>

                <!-- 操作 -->
                <section class="detail-panel" v-if="report.status !== 'done'">
                    <div class="action-buttons">
                        <el-button type="danger" plain @click="handleCancel">
                            撤销举报
                        </el-button>
                        <label class="upload-label">
                            <input type="file" @change="onFileChange" :disabled="uploading" />
                            <el-button type="primary" :loading="uploading">
                                上传附件
                            </el-button>
                        </label>
                    </div>
                </section>
            </template>
        </div>
    </main>
</template>

<style scoped>
.report-detail-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.report-detail-container {
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

.report-info-text {
    margin: 0;
    color: #1e2a26;
    font-size: 14px;
    line-height: 1.7;
    white-space: pre-wrap;
}

.action-buttons {
    display: flex;
    gap: 12px;
    align-items: center;
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
