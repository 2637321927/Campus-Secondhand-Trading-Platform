<script setup lang="ts">
import {
  computed,
  onMounted,
  ref
} from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { createAppeal, getAppealTypes } from '../../api/modules/report'
import type { AppealTypeDto } from '../../types/api/report'
import { useAuthStore } from '../../stores/auth'
import { getApiErrorMessage } from '../../utils/error'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const appealTypes = ref<AppealTypeDto[]>([])
const selectedType = ref('')
const reasonText = ref('')
const infoText = ref('')
const submitting = ref(false)

/** 从路由 query 预填申诉对象（可选） */
const presetTargetType = ref<string | null>(
  (route.query.targetType as string) || null
)
const presetTargetId = ref<number | null>(
  route.query.targetId ? Number(route.query.targetId) : null
)

const selectedTypeMeta = computed(() =>
  appealTypes.value.find((t) => t.code === selectedType.value)
)

const canSubmit = computed(
  () => selectedType.value !== '' && reasonText.value.trim() !== ''
)

async function loadAppealTypes(): Promise<void> {
  try {
    const response = await getAppealTypes()

    appealTypes.value = response.data ?? []

    if (appealTypes.value.length > 0 && !selectedType.value) {
      selectedType.value = appealTypes.value[0].code
    }
  } catch (error) {
    console.error('申诉类型加载失败：', error)
  }
}

async function handleSubmit(): Promise<void> {
  if (!canSubmit.value || submitting.value) {
    return
  }

  if (!authStore.isLoggedIn) {
    ElMessage.warning('请先登录后再申诉')

    void router.push({
      name: 'login',
      query: { redirect: route.fullPath }
    })

    return
  }

  submitting.value = true

  try {
    await createAppeal({
      // 后端 reason 上限 100 字，放原因摘要；详细描述放 info
      reason: reasonText.value.trim().slice(0, 100),
      info: infoText.value.trim() || undefined,
      targetType: selectedType.value,
      targetId: presetTargetId.value ?? undefined
    })

    ElMessage.success('申诉已提交，我们会尽快处理')

    void router.push({ name: 'user-reports' })
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '申诉提交失败，请稍后重试')
    )

    console.error('申诉提交失败：', error)
  } finally {
    submitting.value = false
  }
}

function goBack(): void {
  router.back()
}

onMounted(() => {
  void loadAppealTypes()
})
</script>

<template>
  <main class="appeal-page">
    <div class="appeal-container">
      <header class="appeal-header">
        <button
          class="back-button"
          type="button"
          @click="goBack"
        >
          返回
        </button>

        <h1>发起申诉</h1>

        <p class="appeal-subtitle">
          对平台的处理决定有异议时可以发起申诉，管理员会重新核实。
        </p>
      </header>

      <section class="form-section">
        <div class="form-item">
          <h3 class="section-label">申诉类型</h3>

          <el-select
            v-model="selectedType"
            placeholder="请选择申诉类型"
            class="type-select"
          >
            <el-option
              v-for="item in appealTypes"
              :key="item.code"
              :label="item.name"
              :value="item.code"
            />
          </el-select>

          <p
            v-if="selectedTypeMeta"
            class="type-description"
          >
            {{ selectedTypeMeta.description }}
          </p>
        </div>

        <div class="form-item">
          <h3 class="section-label">申诉原因</h3>

          <el-input
            v-model="reasonText"
            maxlength="100"
            show-word-limit
            placeholder="一句话概括申诉原因（100 字内）"
          />
        </div>

        <div class="form-item">
          <h3 class="section-label">详细说明（可选）</h3>

          <el-input
            v-model="infoText"
            type="textarea"
            :rows="5"
            maxlength="500"
            show-word-limit
            placeholder="补充事情经过、相关订单或商品信息等"
          />
        </div>

        <p
          v-if="presetTargetType && presetTargetId"
          class="preset-note"
        >
          本次申诉关联对象：{{ presetTargetType }} #{{ presetTargetId }}
        </p>

        <el-button
          class="submit-button"
          type="primary"
          size="large"
          :loading="submitting"
          :disabled="!canSubmit"
          @click="handleSubmit"
        >
          {{ submitting ? '提交中…' : '提交申诉' }}
        </el-button>
      </section>
    </div>
  </main>
</template>

<style scoped>
.appeal-page {
  min-height: calc(100vh - 72px);
  padding: 24px;
  background: #f5f7f6;
  color: #1e2a26;
}

.appeal-container {
  max-width: 720px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.appeal-header {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.appeal-header h1 {
  margin: 0;
  font-size: 22px;
}

.appeal-subtitle {
  margin: 0;
  color: #6c7a74;
  font-size: 13px;
}

.back-button {
  align-self: flex-start;
  padding: 6px 12px;
  color: #24735b;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.back-button:hover {
  background: #eef4f2;
}

.section-label {
  margin: 0 0 10px;
  font-size: 14px;
  font-weight: 600;
}

.form-section {
  padding: 20px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 14px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.type-select {
  width: 100%;
}

.type-description {
  margin: 8px 0 0;
  color: #6c7a74;
  font-size: 12px;
}

.preset-note {
  margin: 0;
  padding: 10px 14px;
  color: #24735b;
  font-size: 13px;
  background: #eef4f2;
  border-radius: 8px;
}

.submit-button {
  align-self: flex-end;
  background: #24735b;
  border-color: #24735b;
}
</style>
