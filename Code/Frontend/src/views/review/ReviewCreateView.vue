<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { createReview } from '../../api/modules/review'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()

const orderId = Number(route.params.orderId)

const loading = ref(false)
const submitting = ref(false)
const rating = ref(5)
const info = ref('')

async function handleSubmit(): Promise<void> {
    if (rating.value < 1 || rating.value > 5) {
        ElMessage.warning('请选择评分')
        return
    }

    submitting.value = true
    try {
        await createReview(orderId, {
            rating: rating.value,
            info: info.value || null
        })
        ElMessage.success('评价提交成功')
        router.push({
            name: 'order-detail',
            params: { orderId }
        })
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '评价提交失败'))
        console.error('评价提交失败：', error)
    } finally {
        submitting.value = false
    }
}

onMounted(() => {
    if (!orderId || Number.isNaN(orderId)) {
        ElMessage.error('订单ID无效')
        router.back()
    }
})
</script>

<template>
    <main class="review-create-page">
        <div class="review-create-container">
            <!-- 返回按钮 -->
            <el-button text @click="router.back()">
                ← 返回
            </el-button>

            <!-- 页面头部 -->
            <header class="page-header">
                <p class="page-eyebrow">REVIEW</p>
                <h1>评价订单</h1>
                <p class="page-description">
                    订单号：{{ orderId }}
                </p>
            </header>

            <!-- 评价表单 -->
            <section class="review-panel">
                <el-form label-width="80px">
                    <el-form-item label="评分">
                        <el-rate
                            v-model="rating"
                            :max="5"
                            :colors="['#D9544D', '#F3A95F', '#24735B']"
                        />
                    </el-form-item>

                    <el-form-item label="评价">
                        <el-input
                            v-model="info"
                            type="textarea"
                            :rows="5"
                            placeholder="请输入评价内容（选填）"
                            maxlength="500"
                            show-word-limit
                        />
                    </el-form-item>

                    <el-form-item>
                        <el-button
                            type="primary"
                            :loading="submitting"
                            @click="handleSubmit"
                        >
                            提交评价
                        </el-button>
                        <el-button @click="router.back()">
                            取消
                        </el-button>
                    </el-form-item>
                </el-form>
            </section>
        </div>
    </main>
</template>

<style scoped>
.review-create-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.review-create-container {
    width: 100%;
    max-width: 640px;
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

.page-description {
    margin: 8px 0 0;
    color: #6c7a74;
    font-size: 14px;
}

.review-panel {
    padding: 28px 30px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
}
</style>
