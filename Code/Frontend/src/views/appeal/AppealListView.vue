<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { getMyAppeals } from '../../api/modules/appeal'
import type { WorkOrderDto } from '../../types/api/appeal'
import { getApiErrorMessage } from '../../utils/error'

const router = useRouter()

const loading = ref(false)
const errorMessage = ref('')
const appealList = ref<WorkOrderDto[]>([])

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

async function loadAppeals(): Promise<void> {
    loading.value = true
    errorMessage.value = ''

    try {
        const response = await getMyAppeals()
        appealList.value = response.data ?? []
    } catch (error) {
        appealList.value = []
        errorMessage.value = getApiErrorMessage(error, '申诉列表加载失败，请稍后重试')
        console.error('申诉列表加载失败：', error)
    } finally {
        loading.value = false
    }
}

function goToDetail(appealId: number): void {
    router.push({
        name: 'appeal-detail',
        params: { appealId }
    })
}

onMounted(() => {
    void loadAppeals()
})
</script>

<template>
    <main class="appeal-list-page">
        <div class="appeal-list-container">
            <!-- 页面头部 -->
            <header class="page-header">
                <div>
                    <p class="page-eyebrow">MY APPEALS</p>
                    <h1>我的申诉</h1>
                    <p class="page-description">
                        查看和管理你发起的申诉记录。
                    </p>
                </div>
                <el-button type="primary" @click="router.push({ name: 'appeal-create' })">
                    发起申诉
                </el-button>
            </header>

            <!-- 加载状态 -->
            <section v-if="loading" class="appeal-panel">
                <div v-for="i in 3" :key="i" class="skeleton-card">
                    <el-skeleton :rows="3" animated />
                </div>
            </section>

            <!-- 错误状态 -->
            <el-result
                v-else-if="errorMessage"
                icon="error"
                title="申诉列表加载失败"
                :sub-title="errorMessage"
            >
                <template #extra>
                    <el-button type="primary" @click="loadAppeals">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 空数据 -->
            <el-empty
                v-else-if="appealList.length === 0"
                description="暂无申诉记录"
            >
                <el-button type="primary" plain @click="router.push({ name: 'appeal-create' })">
                    发起申诉
                </el-button>
            </el-empty>

            <!-- 申诉列表 -->
            <div v-else class="appeal-list">
                <div
                    v-for="appeal in appealList"
                    :key="appeal.id"
                    class="appeal-card"
                    @click="goToDetail(appeal.id)"
                >
                    <div class="appeal-card-left">
                        <h3 class="appeal-reason">{{ appeal.reason }}</h3>
                        <p class="appeal-meta">
                            申诉ID：{{ appeal.id }}
                        </p>
                        <p class="appeal-meta">
                            时间：{{ formatDateTime(appeal.createTime) }}
                        </p>
                    </div>
                    <div class="appeal-card-right">
                        <el-tag
                            :type="statusTagType[appeal.status] ?? 'info'"
                            effect="plain"
                        >
                            {{ statusTextMap[appeal.status] ?? appeal.status }}
                        </el-tag>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<style scoped>
.appeal-list-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.appeal-list-container {
    width: 100%;
    max-width: 800px;
    margin: 0 auto;
}

.page-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
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

.appeal-panel {
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

.appeal-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.appeal-card {
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

.appeal-card:hover {
    border-color: #3e9b79;
    box-shadow: 0 8px 20px rgb(36 115 91 / 10%);
}

.appeal-card-left {
    flex: 1;
    min-width: 0;
}

.appeal-reason {
    margin: 0 0 8px;
    color: #1e2a26;
    font-size: 16px;
    font-weight: 600;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.appeal-meta {
    margin: 0 0 4px;
    color: #6c7a74;
    font-size: 13px;
}

.appeal-card-right {
    display: flex;
    flex-shrink: 0;
    align-items: center;
}

@media (max-width: 760px) {
    .page-header {
        flex-direction: column;
        align-items: flex-start;
    }
}
</style>
