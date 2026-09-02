<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import {
    createReport,
    getProductReportInfo,
    getReportReasons,
    getUserReportInfo
} from '../../api/modules/report'
import type {
    CreateReportDto,
    ReportProductInfoDto,
    ReportReason,
    ReportUserInfoDto
} from '../../types/api/report'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()

const targetType = computed(() => (route.query.type as string) || '')
const targetId = computed(() => Number(route.query.id) || 0)

const loading = ref(false)
const submitting = ref(false)
const reasons = ref<ReportReason[]>([])
const productInfo = ref<ReportProductInfoDto | null>(null)
const userInfo = ref<ReportUserInfoDto | null>(null)

const reason = ref('')
const info = ref('')

async function loadReasons(): Promise<void> {
    try {
        const response = await getReportReasons()
        reasons.value = response.data ?? []
        if (reasons.value.length > 0 && !reason.value) {
            reason.value = reasons.value[0].code
        }
    } catch (error) {
        console.error('举报原因加载失败：', error)
    }
}

async function loadTargetInfo(): Promise<void> {
    if (!targetType.value || !targetId.value) return

    if (targetType.value === 'product') {
        try {
            const response = await getProductReportInfo(targetId.value)
            productInfo.value = response.data
        } catch (error) {
            console.error('被举报商品信息加载失败：', error)
        }
    } else if (targetType.value === 'user') {
        try {
            const response = await getUserReportInfo(targetId.value)
            userInfo.value = response.data
        } catch (error) {
            console.error('被举报用户信息加载失败：', error)
        }
    }
}

async function handleSubmit(): Promise<void> {
    if (!reason.value) {
        ElMessage.warning('请选择举报原因')
        return
    }

    if (!targetType.value || !targetId.value) {
        ElMessage.warning('举报对象信息缺失')
        return
    }

    submitting.value = true
    try {
        const data: CreateReportDto = {
            targetType: targetType.value,
            targetId: targetId.value,
            reason: reason.value,
            info: info.value.trim() || null
        }

        if (targetType.value === 'product' && productInfo.value) {
            data.accusedId = productInfo.value.sellerId
            data.productId = targetId.value
        } else if (targetType.value === 'user' && userInfo.value) {
            data.accusedId = userInfo.value.userId
        }

        await createReport(data)
        ElMessage.success('举报提交成功')
        router.push({ name: 'report-list' })
    } catch (error) {
        ElMessage.error(getApiErrorMessage(error, '举报提交失败'))
        console.error('举报提交失败：', error)
    } finally {
        submitting.value = false
    }
}

onMounted(() => {
    void loadReasons()
    void loadTargetInfo()
})
</script>

<template>
    <main class="report-create-page">
        <div class="report-create-container">
            <!-- 返回按钮 -->
            <el-button text @click="router.back()">
                ← 返回
            </el-button>

            <!-- 页面头部 -->
            <header class="page-header">
                <p class="page-eyebrow">REPORT</p>
                <h1>发起举报</h1>
            </header>

            <!-- 举报对象信息 -->
            <section class="report-panel" v-if="productInfo || userInfo">
                <h2 class="panel-title">举报对象</h2>
                <div class="target-info" v-if="productInfo">
                    <p><strong>商品名称：</strong>{{ productInfo.name }}</p>
                    <p><strong>卖家ID：</strong>{{ productInfo.sellerId }}</p>
                    <p><strong>商品状态：</strong>{{ productInfo.status }}</p>
                </div>
                <div class="target-info" v-else-if="userInfo">
                    <p><strong>用户名：</strong>{{ userInfo.userName }}</p>
                    <p v-if="userInfo.profile"><strong>简介：</strong>{{ userInfo.profile }}</p>
                </div>
            </section>

            <!-- 举报表单 -->
            <section class="report-panel">
                <h2 class="panel-title">举报内容</h2>
                <el-form label-width="100px">
                    <el-form-item label="举报原因">
                        <el-select v-model="reason" placeholder="请选择举报原因">
                            <el-option
                                v-for="r in reasons"
                                :key="r.code"
                                :label="r.name"
                                :value="r.code"
                            />
                        </el-select>
                    </el-form-item>

                    <el-form-item label="详细说明">
                        <el-input
                            v-model="info"
                            type="textarea"
                            :rows="5"
                            placeholder="请详细描述举报原因（选填）"
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
                            提交举报
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
.report-create-page {
    min-height: calc(100vh - 72px);
    padding: 36px 24px 64px;
    background: #f5f7f6;
    color: #1e2a26;
}

.report-create-container {
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

.report-panel {
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

.target-info p {
    margin: 0 0 6px;
    color: #46534d;
    font-size: 14px;
    line-height: 1.7;
}
</style>
