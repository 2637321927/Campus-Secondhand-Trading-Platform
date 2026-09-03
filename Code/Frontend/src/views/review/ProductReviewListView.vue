<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { getProductReviews } from '../../api/modules/review'
import { deleteReview, replyReview } from '../../api/modules/review'
import type { ReviewDto } from '../../types/api/review'
import { getApiErrorMessage } from '../../utils/error'
import { useAuthStore } from '../../stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const productId = computed(() => Number(route.params.productId))

const loading = ref(false)
const errorMessage = ref('')
const reviewList = ref<ReviewDto[]>([])

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

async function loadReviews(): Promise<void> {
    loading.value = true
    errorMessage.value = ''

    try {
        const response = await getProductReviews(productId.value)
        reviewList.value = response.data ?? []
    } catch (error) {
        reviewList.value = []
        errorMessage.value = getApiErrorMessage(error, '评价列表加载失败，请稍后重试')
        console.error('商品评价加载失败：', error)
    } finally {
        loading.value = false
    }
}

// 回复弹窗
const replyDialogVisible = ref(false)
const replyTargetId = ref(0)
const replyContent = ref('')

function openReplyDialog(reviewId: number): void {
    replyTargetId.value = reviewId
    replyContent.value = ''
    replyDialogVisible.value = true
}

async function handleReply(): Promise<void> {
    if (!replyContent.value.trim()) {
        ElMessage.warning('请输入回复内容')
        return
    }

    try {
        await replyReview(replyTargetId.value, {
            replyInfo: replyContent.value.trim()
        })
        ElMessage.success('回复成功')
        replyDialogVisible.value = false
        await loadReviews()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '回复失败'))
        console.error('回复评价失败：', error)
    }
}

async function handleDelete(reviewId: number): Promise<void> {
    try {
        await ElMessageBox.confirm(
            '确定删除此评价吗？',
            '删除评价',
            { type: 'warning', confirmButtonText: '确定删除', cancelButtonText: '取消' }
        )
    } catch {
        return
    }

    try {
        await deleteReview(reviewId)
        ElMessage.success('评价已删除')
        await loadReviews()
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '删除评价失败'))
        console.error('删除评价失败：', error)
    }
}

onMounted(() => {
    void loadReviews()
})
</script>

<template>
    <main class="review-list-page">
        <div class="review-list-container">
            <!-- 返回按钮 -->
            <el-button text @click="router.back()">
                ← 返回
            </el-button>

            <!-- 页面头部 -->
            <header class="page-header">
                <p class="page-eyebrow">PRODUCT REVIEWS</p>
                <h1>商品评价</h1>
            </header>

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
                    <el-button type="primary" @click="loadReviews">
                        重新加载
                    </el-button>
                </template>
            </el-result>

            <!-- 空数据 -->
            <el-empty
                v-else-if="reviewList.length === 0"
                description="暂无评价"
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
                            {{ review.reviewerName ?? '匿名用户' }}
                        </span>
                        <el-rate :model-value="review.rating" disabled size="small" />
                    </div>
                    <p class="review-info" v-if="review.info">
                        {{ review.info }}
                    </p>
                    <p class="review-time">
                        {{ formatDateTime(review.reviewTime) }}
                    </p>

                    <!-- 回复内容 -->
                    <div v-if="review.replyInfo" class="review-reply">
                        <span class="reply-label">卖家回复：</span>
                        <span>{{ review.replyInfo }}</span>
                        <span class="reply-time" v-if="review.replyTime">
                            {{ formatDateTime(review.replyTime) }}
                        </span>
                    </div>

                    <!-- 操作按钮 -->
                    <div class="review-actions" v-if="authStore.isLoggedIn">
                        <el-button
                            v-if="review.revieweeId === authStore.currentUser?.userId && !review.replyInfo"
                            type="primary"
                            size="small"
                            @click="openReplyDialog(review.reviewId)"
                        >
                            回复
                        </el-button>
                        <el-button
                            v-if="review.reviewerId === authStore.currentUser?.userId"
                            type="danger"
                            size="small"
                            plain
                            @click="handleDelete(review.reviewId)"
                        >
                            删除
                        </el-button>
                    </div>
                </div>
            </div>
        </div>

        <!-- 回复弹窗 -->
        <el-dialog v-model="replyDialogVisible" title="回复评价" width="440px">
            <el-input
                v-model="replyContent"
                type="textarea"
                :rows="4"
                placeholder="请输入回复内容"
                maxlength="200"
                show-word-limit
            />
            <template #footer>
                <el-button @click="replyDialogVisible = false">取消</el-button>
                <el-button type="primary" @click="handleReply">确认回复</el-button>
            </template>
        </el-dialog>
    </main>
</template>

<style scoped>
.review-list-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.review-list-container {
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

.reply-time {
    display: block;
    margin-top: 4px;
    color: #6c7a74;
    font-size: 12px;
}

.review-actions {
    display: flex;
    gap: 8px;
    margin-top: 12px;
}
</style>
