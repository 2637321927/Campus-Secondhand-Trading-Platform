<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type {
  FormInstance,
  FormRules
} from 'element-plus'
import { changePassword } from '../../api/modules/auth'
import { getApiErrorMessage } from '../../utils/error'

interface PasswordForm {
  oldPassword: string
  newPassword: string
  confirmPassword: string
}

const router = useRouter()

const formRef = ref<FormInstance>()
const submitting = ref(false)

const form = reactive<PasswordForm>({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const rules: FormRules<PasswordForm> = {
  oldPassword: [
    {
      required: true,
      message: '请输入原密码',
      trigger: 'blur'
    }
  ],
  newPassword: [
    {
      required: true,
      message: '请输入新密码',
      trigger: 'blur'
    },
    {
      min: 6,
      message: '新密码长度不能少于 6 位',
      trigger: 'blur'
    }
  ],
  confirmPassword: [
    {
      required: true,
      message: '请再次输入新密码',
      trigger: 'blur'
    },
    {
      validator: (_rule, value, callback) => {
        if (value !== form.newPassword) {
          callback(new Error('两次输入的密码不一致'))
          return
        }

        callback()
      },
      trigger: 'blur'
    }
  ]
}

function resetForm(): void {
  form.oldPassword = ''
  form.newPassword = ''
  form.confirmPassword = ''

  formRef.value?.clearValidate()
}

async function handleSubmit(): Promise<void> {
  if (!formRef.value) {
    return
  }

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  submitting.value = true

  try {
    await changePassword({
      oldPassword: form.oldPassword,
      newPassword: form.newPassword
    })

    ElMessage.success('密码修改成功')

    resetForm()
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '密码修改失败，请稍后重试')
    )

    console.error('密码修改失败：', error)
  } finally {
    submitting.value = false
  }
}

function goBack(): void {
  void router.push({ name: 'user-overview' })
}
</script>

<template>
  <main class="settings-page">
    <div class="settings-container">
      <!-- 页面头部 -->
      <header class="page-header">
        <div>
          <p class="page-eyebrow">ACCOUNT SETTINGS</p>

          <h1>账号设置</h1>

          <p class="page-description">
            修改你的登录密码，保障账号安全。
          </p>
        </div>

        <el-button @click="goBack">
          返回个人中心
        </el-button>
      </header>

      <section class="settings-panel">
        <h2 class="panel-title">修改密码</h2>

        <el-form
          ref="formRef"
          class="settings-form"
          :model="form"
          :rules="rules"
          label-position="top"
        >
          <el-form-item
            label="原密码"
            prop="oldPassword"
          >
            <el-input
              v-model="form.oldPassword"
              type="password"
              show-password
              autocomplete="current-password"
              placeholder="请输入原密码"
            />
          </el-form-item>

          <el-form-item
            label="新密码"
            prop="newPassword"
          >
            <el-input
              v-model="form.newPassword"
              type="password"
              show-password
              autocomplete="new-password"
              placeholder="至少 6 位"
            />
          </el-form-item>

          <el-form-item
            label="确认新密码"
            prop="confirmPassword"
          >
            <el-input
              v-model="form.confirmPassword"
              type="password"
              show-password
              autocomplete="new-password"
              placeholder="再次输入新密码"
            />
          </el-form-item>

          <div class="form-actions">
            <el-button
              type="primary"
              :loading="submitting"
              :disabled="submitting"
              @click="handleSubmit"
            >
              确认修改
            </el-button>

            <el-button
              :disabled="submitting"
              @click="resetForm"
            >
              重置
            </el-button>
          </div>
        </el-form>
      </section>
    </div>
  </main>
</template>

<style scoped>
.settings-page {
  min-height: calc(100vh - 72px);
  padding: 36px 24px 64px;
  background: #f5f7f6;
  color: #1e2a26;
}

.settings-container {
  width: 100%;
  max-width: 680px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 20px;
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

.settings-panel {
  padding: 30px 32px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.panel-title {
  margin: 0 0 22px;
  color: #1e2a26;
  font-size: 20px;
}

.settings-form {
  max-width: 460px;
}

.form-actions {
  display: flex;
  gap: 12px;
  margin-top: 8px;
}

@media (max-width: 640px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
