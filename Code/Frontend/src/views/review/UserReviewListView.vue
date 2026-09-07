<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getReputationSummary, getUserReceivedReviews, getUserGivenReviews } from '../../api/modules/review'
import type { ReputationSummaryDto, ReviewDto } from '../../types/api/review'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()

const userId = computed(() => Number(route.params.userId))
const tab = ref<'received' | 'given'>('received')

const loading = ref(false)
const errorMessage = ref('')
const reviewList = ref<ReviewDto[]>([])
const reputation = ref<ReputationSummaryDto | null>(null)

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

async function loadData(): Promise<void> {
    if (!userId.value || Number.isNaN(userId.value)) {
        errorMessage.value = '用户ID无效'
        return
    }

    loading.value = true
    errorMessage.value = ''

    try {
        const [reviewsResponse, reputationResponse] = await Promise.allSettled([
            tab.value === 'received'
                ? getUserReceivedReviews(userId.value)
                : getUserGivenReviews(userId.value),
            getReputationSummary(userId.value)
        ])

        if (reviewsResponse.status === 'fulfilled') {
            reviewList.value = reviewsResponse.value.data ?? []
        } else {
            throw reviewsResponse.reason
        }

        if (reputationResponse.status === 'fulfilled') {
            reputation.value = reputationResponse.value.data
        }
    } catch (error) {
        reviewList.value = []
        errorMessage.value = getApiErrorMessage(error, '评价列表加载失败，请稍后重试')
        console.error('用户评价加载失败：', error)
    } finally {
        loading.value = false
    }
}

async function handleTabChange(): Promise<void> {
    await loadData()
}

onMounted(() => {
    void loadData()
})
</script>

<template>
    <main class="user-review-page">
        <div class="user-review-container">
            <!-- 返回按钮 -->
            <el-button text @click="router.back()">
                ← 返回
            </el-button>

            <!-- 页面头部 -->
            <header class="page-header">
                <p class="page-eyebrow">USER REVIEWS</p>
                <h1>用户评价</h1>
            </header>

            <!-- 信誉概览 -->
            <section class="reputation-panel" v-if="reputation">
                <div class="reputation-grid">
                    <div class="rep-item">
                        <span class="rep-value">{{ reputation.credit }}</span>
                        <span class="rep-label">信誉分</span>
                    </div>
                    <div class="rep-item">
                        <span class="rep-value">{{ reputation.totalReviews }}</span>
                        <span class="rep-label">总评价数</span>
                    </div>
                    <div class="rep-item">
                        <span class="rep-value">{{ reputation.goodRate }}%</span>
                        <span class="rep-label">好评率</span>
                    </div>
                    <div class="rep-item">
                        <span class="rep-value">{{ reputation.averageRating }}</span>
                        <span class="rep-label">平均评分</span>
                    </div>
                </div>
            </section>

            <!-- Tab切换 -->
            <div class="tab-bar">
                <el-radio-group v-model="tab" @change="handleTabChange">
                    <el-radio-button value="received">收到的评价</el-radio-button>
                    <el-radio-button value="given">发出的评价</el-radio-button>
                </el-radio-group>
            </div>

            <!-- 加载状态 -->
            <section v-if="loading" class="review-panel">
                <div v-for="i in 3" :key="i" class="skeleton-card">
                    <el-skeleton :rows="3" animated />
                </div>
            </section>

            <!-- 错误状态 -->
            <el-result
                v-else-if="errorMessage"
                icon="error"
                title="评价加载失败"
                :sub-title="errorMessage"
            >
                <template #extra>
                    <el-button type="primary" @click="loadData">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 空数据 -->
            <el-empty
                v-else-if="reviewList.length === 0"
                :description="tab === 'received' ? '暂无收到的评价' : '暂无发出的评价'"
            />

            <!-- 评价列表 -->
            <div v-else class="review-list">
                <div
                    v-for="review in reviewList"
                    :key="review.reviewId"
                    class="review-card"
                >
                    <div class="review-header">
                        <span class="reviewer-name">
                            {{ tab === 'received' ? (review.reviewerName ?? '匿名用户') : (review.revieweeName ?? '用户') }}
                        </span>
                        <el-rate :model-value="review.rating" disabled size="small" />
                    </div>
                    <p class="review-info" v-if="review.info">{{ review.info }}</p>
                    <p class="review-time">{{ formatDateTime(review.reviewTime) }}</p>
                    <div v-if="review.replyInfo" class="review-reply">
                        <span class="reply-label">回复：</span>
                        <span>{{ review.replyInfo }}</span>
                    </div>
                </div>
            </div>
        </div>
    </main>
</template>

<style scoped>
.user-review-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.user-review-container {
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

.page-header h1 {
    margin: 0;
    color: #1e2a26;
    font-size: 28px;
    line-height: 1.25;
}

.reputation-panel {
    margin-bottom: 20px;
    padding: 24px 28px;
    background: linear-gradient(135deg, #ffffff 0%, #edf5f1 100%);
    border: 1px solid #e3e9e6;
    border-radius: 16px;
}

.reputation-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 16px;
}

.rep-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
}

.rep-value {
    color: #24735b;
    font-size: 24px;
    font-weight: 700;
}

.rep-label {
    color: #6c7a74;
    font-size: 13px;
}

.tab-bar {
    margin-bottom: 20px;
}

.review-panel {
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

.review-list {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.review-card {
    padding: 22px 26px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
}

.review-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 10px;
}

.reviewer-name {
    color: #1e2a26;
    font-size: 15px;
    font-weight: 600;
}

.review-info {
    margin: 0 0 8px;
    color: #1e2a26;
    font-size: 14px;
    line-height: 1.7;
}

.review-time {
    margin: 0 0 8px;
    color: #6c7a74;
    font-size: 13px;
}

.review-reply {
    padding: 12px 16px;
    background: #f5f7f6;
    border-radius: 10px;
    font-size: 14px;
    line-height: 1.7;
}

.reply-label {
    color: #3e9b79;
    font-weight: 600;
}

@media (max-width: 760px) {
    .reputation-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}
</style>
