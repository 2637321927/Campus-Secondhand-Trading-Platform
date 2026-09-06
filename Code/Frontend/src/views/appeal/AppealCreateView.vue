<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { createAppeal } from '../../api/modules/appeal'
import { getApiErrorMessage } from '../../utils/error'

const router = useRouter()

const submitting = ref(false)
const reason = ref('')
const info = ref('')
const appealAgainstId = ref<number | null>(null)

async function handleSubmit(): Promise<void> {
    if (!reason.value.trim()) {
        ElMessage.warning('请输入申诉原因')
        return
    }

    submitting.value = true
    try {
        await createAppeal({
            reason: reason.value.trim(),
            info: info.value.trim() || null,
            appealAgainstId: appealAgainstId.value,
            targetType: null,
            targetId: null
        })
        ElMessage.success('申诉提交成功')
        router.push({ name: 'appeal-list' })
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '申诉提交失败'))
        console.error('申诉提交失败：', error)
    } finally {
        submitting.value = false
    }
}

onMounted(() => {
    // 如果从举报详情跳来，会带 query.appealAgainstId
    const queryId = router.currentRoute.value.query.appealAgainstId
    if (queryId) {
        appealAgainstId.value = Number(queryId) || null
    }
})
</script>

<template>
    <main class="appeal-create-page">
        <div class="appeal-create-container">
            <!-- 返回按钮 -->
            <el-button text @click="router.back()">
                ← 返回
            </el-button>

            <!-- 页面头部 -->
            <header class="page-header">
                <p class="page-eyebrow">APPEAL</p>
                <h1>发起申诉</h1>
            </header>

            <!-- 申诉表单 -->
            <section class="appeal-panel">
                <el-form label-width="100px">
                    <el-form-item label="申诉原因">
                        <el-input
                            v-model="reason"
                            placeholder="请输入申诉原因"
                            maxlength="100"
                            show-word-limit
                        />
                    </el-form-item>

                    <el-form-item label="申诉说明">
                        <el-input
                            v-model="info"
                            type="textarea"
                            :rows="5"
                            placeholder="请详细说明申诉理由"
                            maxlength="500"
                            show-word-limit
                        />
                    </el-form-item>

                    <el-form-item label="关联工单ID" v-if="appealAgainstId">
                        <el-input :model-value="appealAgainstId" disabled />
                    </el-form-item>

                    <el-form-item>
                        <el-button
                            type="primary"
                            :loading="submitting"
                            @click="handleSubmit"
                        >
                            提交申诉
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
.appeal-create-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.appeal-create-container {
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

.appeal-panel {
    padding: 28px 30px;
    background: #ffffff;
    border: 1px solid #e3e9e6;
    border-radius: 16px;
}
</style>
