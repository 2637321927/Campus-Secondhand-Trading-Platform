<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getMyReports, cancelReport } from '../../api/modules/report'
import type { WorkOrderDto } from '../../types/api/report'
import { getApiErrorMessage } from '../../utils/error'

const router = useRouter()

const loading = ref(false)
const errorMessage = ref('')
const reportList = ref<WorkOrderDto[]>([])

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

async function loadReports(): Promise<void> {
    loading.value = true
    errorMessage.value = ''

    try {
        const response = await getMyReports()
        reportList.value = response.data ?? []
    } catch (error) {
        reportList.value = []
        errorMessage.value = getApiErrorMessage(error, '举报列表加载失败，请稍后重试')
        console.error('举报列表加载失败：', error)
    } finally {
        loading.value = false
    }
}

function goToDetail(reportId: number): void {
    router.push({
        name: 'report-detail',
        params: { reportId }
    })
}

async function handleCancel(reportId: number): Promise<void> {
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
        await cancelReport(reportId)
        ElMessage.success('举报已撤销')
        await loadReports()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '撤销举报失败'))
        console.error('撤销举报失败：', error)
    }
}

onMounted(() => {
    void loadReports()
})
</script>

<template>
    <main class="report-list-page">
        <div class="report-list-container">
            <!-- 页面头部 -->
            <header class="page-header">
                <p class="page-eyebrow">MY REPORTS</p>
                <h1>我的举报</h1>
                <p class="page-description">
                    查看和管理你发起的举报记录。
                </p>
            </header>

            <!-- 加载状态 -->
            <section v-if="loading" class="report-panel">
                <div v-for="i in 3" :key="i" class="skeleton-card">
                    <el-skeleton :rows="3" animated />
                </div>
            </section>

            <!-- 错误状态 -->
            <el-result
                v-else-if="errorMessage"
                icon="error"
                title="举报列表加载失败"
                :sub-title="errorMessage"
            >
                <template #extra>
                    <el-button type="primary" @click="loadReports">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 空数据 -->
            <el-empty
                v-else-if="reportList.length === 0"
                description="暂无举报记录"
            />

            <!-- 举报列表 -->
            <div v-else class="report-list">
                <div
                    v-for="report in reportList"
                    :key="report.id"
                    class="report-card"
                    @click="goToDetail(report.id)"
                >
                    <div class="report-card-left">
                        <h3 class="report-reason">
                            {{ reasonTextMap[report.reason] ?? report.reason }}
                        </h3>
                        <p class="report-meta">
                            举报ID：{{ report.id }}
                        </p>
                        <p class="report-meta">
                            对象：{{ report.targetType ?? '未知' }}
                        </p>
                        <p class="report-meta">
                            时间：{{ formatDateTime(report.createTime) }}
                        </p>
                    </div>
                    <div class="report-card-right">
                        <el-tag
                            :type="statusTagType[report.status] ?? 'info'"
                            effect="plain"
                        >
                            {{ statusTextMap[report.status] ?? report.status }}
                        </el-tag>
                        <el-button
                            v-if="report.status !== 'done'"
                            type="danger"
                            size="small"
                            plain
                            @click.stop="handleCancel(report.id)"
                        >
                            撤销
                        </el-button>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<style scoped>
.report-list-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.report-list-container {
    width: 100%;
    max-width: 800px;
    margin: 0 auto;
}

.page-header {
    margin-bottom: 24px;
}

.page-eyebrow {
    margin: 0 0 8px;
    color: #3e9b79;
    font-size: 12px;
    font-weight: 700;
    letter-spacing: 1.6px;
}

.page-header h1 {
    margin: 0;
    color: #1e2a26;
    font-size: 30px;
    line-height: 1.25;
}

.page-description {
    margin: 10px 0 0;
    color: #6c7a74;
    font-size: 14px;
    line-height: 1.7;
}

.report-panel {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.skeleton-card {
    padding: 20px 24px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
}

.report-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.report-card {
    display: flex;
    padding: 20px 24px;
    align-items: center;
    gap: 20px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
    cursor: pointer;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
}

.report-card:hover {
    border-color: #3e9b79;
    box-shadow: 0 8px 20px rgb(36 115 91 / 10%);
}

.report-card-left {
    flex: 1;
    min-width: 0;
}

.report-reason {
    margin: 0 0 8px;
    color: #1e2a26;
    font-size: 16px;
    font-weight: 600;
}

.report-meta {
    margin: 0 0 4px;
    color: #6c7a74;
    font-size: 13px;
}

.report-card-right {
    display: flex;
    flex-shrink: 0;
    flex-direction: column;
    align-items: flex-end;
    gap: 10px;
}

@media (max-width: 760px) {
    .report-card {
        flex-direction: column;
        align-items: flex-start;
    }

    .report-card-right {
        align-items: flex-start;
    }
}
</style>
