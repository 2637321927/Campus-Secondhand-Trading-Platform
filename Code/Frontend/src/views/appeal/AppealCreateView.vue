<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { createAppeal } from '../../api/modules/appeal'
import { getMyRemovedProducts } from '../../api/modules/user'
import { useAuthStore } from '../../stores/auth'
import type { ProductDto } from '../../types/api/product'
import { getApiErrorMessage } from '../../utils/error'

const router = useRouter()
const authStore = useAuthStore()

const submitting = ref(false)
const reason = ref('')
const info = ref('')
const appealAgainstId = ref<number | null>(null)
const appealType = ref<'product' | 'user' | null>(null)
const removedProducts = ref<ProductDto[]>([])
const loadingProducts = ref(false)
const selectedProductId = ref<number | null>(null)

const currentUserId = computed(() => authStore.currentUser?.userId ?? null)
const accountStatus = computed(() => authStore.currentUser?.accountStatus ?? null)
const accountRestricted = computed(() => accountStatus.value !== null && accountStatus.value !== 0)

const targetId = computed<number | null>(() => {
    if (appealType.value === 'product') return selectedProductId.value
    if (appealType.value === 'user') return currentUserId.value
    return null
})

async function loadRemovedProducts(): Promise<void> {
    loadingProducts.value = true
    try {
        const response = await getMyRemovedProducts()
        removedProducts.value = response.data ?? []
        if (
            selectedProductId.value !== null &&
            !removedProducts.value.some(product => product.productId === selectedProductId.value)
        ) {
            selectedProductId.value = null
        }
    } catch (error) {
        removedProducts.value = []
        ElMessage.error(getApiErrorMessage(error, '下架商品加载失败'))
    } finally {
        loadingProducts.value = false
    }
}

function handleTypeChange(): void {
    selectedProductId.value = null
    if (appealType.value === 'product') {
        void loadRemovedProducts()
    }
}

async function handleSubmit(): Promise<void> {
    if (!reason.value.trim()) {
        ElMessage.warning('请输入申诉原因')
        return
    }

    if (!appealType.value) {
        if (!appealAgainstId.value) {
            ElMessage.warning('请选择申诉类型')
            return
        }
    } else if (appealType.value === 'product') {
        if (!selectedProductId.value) {
            ElMessage.warning('请选择要申诉的下架商品')
            return
        }
    } else {
        if (!currentUserId.value || accountStatus.value === null) {
            ElMessage.warning('无法获取当前用户信息，请刷新后重试')
            return
        }
        if (!accountRestricted.value) {
            ElMessage.warning('账号状态正常，无需申诉')
            return
        }
    }

    submitting.value = true
    try {
        await createAppeal({
            reason: reason.value.trim(),
            info: info.value.trim() || null,
            appealAgainstId: appealAgainstId.value,
            targetType: appealType.value,
            targetId: targetId.value
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

onMounted(async () => {
    await authStore.initializeAuth()

    // 如果从举报详情跳来，会带 query.appealAgainstId
    const queryId = router.currentRoute.value.query.appealAgainstId
    if (queryId) {
        appealAgainstId.value = Number(queryId) || null
    }

    if (appealType.value === 'product') {
        await loadRemovedProducts()
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
                    <el-form-item label="申诉类型">
                        <el-radio-group v-model="appealType" @change="handleTypeChange">
                            <el-radio-button value="product">商品被下架</el-radio-button>
                            <el-radio-button value="user">账号受限</el-radio-button>
                        </el-radio-group>
                    </el-form-item>

                    <el-form-item v-if="appealType === 'product'" label="下架商品">
                        <div class="target-field">
                            <el-select
                                v-model="selectedProductId"
                                :loading="loadingProducts"
                                placeholder="请选择需要申诉的下架商品"
                                clearable
                                filterable
                            >
                                <el-option
                                    v-for="product in removedProducts"
                                    :key="product.productId"
                                    :label="`${product.name}（ID：${product.productId}）`"
                                    :value="product.productId"
                                />
                            </el-select>
                            <span
                                v-if="!loadingProducts && removedProducts.length === 0"
                                class="field-tip"
                            >
                                暂无可申诉的下架商品
                            </span>
                        </div>
                    </el-form-item>

                    <el-form-item v-if="appealType === 'user'" label="当前账号">
                        <div class="target-field">
                            <el-input
                                :model-value="`用户ID：${currentUserId ?? ''}`"
                                disabled
                            />
                            <span
                                v-if="accountStatus !== null && !accountRestricted"
                                class="field-tip"
                            >
                                当前账号状态正常，无需申诉
                            </span>
                        </div>
                    </el-form-item>

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

.target-field {
    align-items: stretch;
    display: flex;
    flex-direction: column;
    gap: 8px;
    width: 100%;
}

.field-tip {
    color: #8a9790;
    font-size: 12px;
    line-height: 1.4;
}
</style>
