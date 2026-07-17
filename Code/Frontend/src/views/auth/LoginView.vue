<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import axios from 'axios'

import { useAuthStore } from '../../stores/auth'
import type { LoginRequest } from '../../types/api/auth'

interface ErrorResponse {
  message?: string
}

interface LoginForm {
  email: string
  password: string
}

const router = useRouter()
const authStore = useAuthStore()

const formRef = ref<FormInstance>()
const form = reactive<LoginForm>({
  email: '',
  password: ''
})

const rules = reactive<FormRules<LoginForm>>({
  email: [
    {
      required: true,
      message: '请输入邮箱',
      trigger: 'blur'
    },
    {
      type: 'email',
      message: '请输入正确的邮箱格式',
      trigger: 'blur'
    }
  ],
  password: [
    {
      required: true,
      message: '请输入密码',
      trigger: 'blur'
    },
    {
      min: 6,
      message: '密码长度不能少于6位',
      trigger: 'blur'
    }
  ]
})

async function handleLogin(): Promise<void> {
  if (!formRef.value) {
    return
  }

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  const requestData: LoginRequest = {
    email: form.email.trim(),
    password: form.password
  }

  try {
    await authStore.loginAction(requestData)
    ElMessage.success('登录成功')
    await router.replace('/')
  } catch (error) {
    console.error('登录失败', error)

    let errorMessage = '登录失败，请检查邮箱和密码'

    if (axios.isAxiosError<ErrorResponse>(error)) {
      if (!error.response) {
        errorMessage = '无法连接后端服务，请确认后端是否已经启动'
      } else if (error.response.status === 401) {
        errorMessage = '邮箱或密码错误'
      } else if (error.response.status === 403) {
        errorMessage = '该账号暂时无法登录'
      } else if (error.response.data?.message) {
        errorMessage = error.response.data.message
      }
    }

    ElMessage.error(errorMessage)
  }
}
</script>

<template>
  <main class="auth-page">
    <section
      class="auth-card"
      aria-label="校园二手交易平台登录"
    >
      <div class="card-header">
        <span class="card-kicker">欢迎回来</span>
        <h1>登录账号</h1>
        <p>登录后继续浏览、收藏和发布校园闲置。</p>
      </div>

      <el-form
        ref="formRef"
        class="auth-form"
        :model="form"
        :rules="rules"
        label-position="top"
        @submit.prevent="handleLogin"
      >
        <el-form-item
          label="邮箱"
          prop="email"
        >
          <el-input
            v-model="form.email"
            size="large"
            placeholder="请输入邮箱"
            autocomplete="email"
            clearable
          />
        </el-form-item>

        <el-form-item
          label="密码"
          prop="password"
        >
          <el-input
            v-model="form.password"
            size="large"
            type="password"
            placeholder="请输入密码"
            autocomplete="current-password"
            show-password
          />
        </el-form-item>

        <div class="form-options">
          <span class="security-note">仅用于校内账号登录</span>

          <router-link to="/forgot-password">
            忘记密码？
          </router-link>
        </div>

        <el-form-item class="submit-item">
          <el-button
            class="primary-button"
            type="primary"
            size="large"
            native-type="submit"
            :loading="authStore.loading"
            :disabled="authStore.loading"
          >
            登录
          </el-button>
        </el-form-item>
      </el-form>

      <div class="auth-switch">
        <span>还没有账号？</span>

        <router-link to="/register">
          立即注册
        </router-link>
      </div>

      <p class="agreement-note">
        登录即表示你已阅读并同意平台规则与隐私说明。
      </p>
    </section>
  </main>
</template>

<style scoped>
.auth-page {
  --brand-primary: #24735b;
  --brand-secondary: #3e9b79;
  --page-bg: #f5f7f6;
  --text-primary: #1e2a26;
  --text-secondary: #6c7a74;
  --border-color: #e3e9e6;
  --danger-color: #d9544d;

  display: flex;
  min-height: 100vh;
  align-items: center;
  justify-content: center;
  padding: 48px 24px;
  box-sizing: border-box;
  background: var(--page-bg);
  color: var(--text-primary);
  font-family: "PingFang SC", "Microsoft YaHei", system-ui, -apple-system, sans-serif;
}

.auth-card {
  width: min(840px, 100%);
  margin: 0 auto;
  padding: 48px 56px;
  box-sizing: border-box;
  background: #fff;
}

.card-header {
  margin-bottom: 38px;
  text-align: center;
}

.card-kicker {
  display: inline-block;
  margin-bottom: 14px;
  color: var(--brand-primary);
  font-size: 15px;
  font-weight: 700;
  letter-spacing: 0.04em;
}

.card-header h2 {
  margin: 0;
  color: var(--text-primary);
  font-size: 36px;
  font-weight: 700;
  line-height: 1.25;
  letter-spacing: -0.02em;
}

.card-header p {
  margin: 14px 0 0;
  color: var(--text-secondary);
  font-size: 15px;
  line-height: 1.8;
}

.auth-form :deep(.el-form-item) {
  margin-bottom: 26px;
}

.auth-form :deep(.el-form-item__label) {
  height: auto;
  padding: 0 0 10px;
  color: var(--text-primary);
  font-size: 15px;
  font-weight: 700;
  line-height: 1.4;
}

.auth-form :deep(.el-input__wrapper) {
  min-height: 50px;
  padding: 0 16px;
  border-radius: 11px;
  background: #fff;
  box-shadow: 0 0 0 1px var(--border-color) inset;
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.auth-form :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px #b9cbc4 inset;
}

.auth-form :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 2px rgb(36 115 91 / 24%) inset;
}

.auth-form :deep(.el-input__inner) {
  color: var(--text-primary);
  font-size: 15px;
}

.auth-form :deep(.el-input__inner::placeholder) {
  color: #a5b0ab;
}

.auth-form :deep(.el-form-item.is-error .el-input__wrapper) {
  box-shadow: 0 0 0 1px var(--danger-color) inset;
}

.auth-form :deep(.el-form-item__error) {
  padding-top: 7px;
  color: var(--danger-color);
  font-size: 13px;
}

.form-options {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin: -2px 0 28px;
  color: var(--text-secondary);
  font-size: 14px;
}

.form-options a,
.auth-switch a {
  color: var(--brand-primary);
  font-weight: 700;
  text-decoration: none;
}

.form-options a:hover,
.auth-switch a:hover {
  color: var(--brand-secondary);
}

.submit-item {
  margin-bottom: 20px !important;
}

.primary-button {
  width: 100%;
  min-height: 50px;
  border: none;
  border-radius: 11px;
  background: var(--brand-primary);
  font-size: 16px;
  font-weight: 700;
  letter-spacing: 0.08em;
  box-shadow: 0 8px 18px rgb(36 115 91 / 16%);
}

.primary-button:hover,
.primary-button:focus {
  background: var(--brand-secondary);
}

.primary-button:active {
  background: #1d604c;
}

.auth-switch {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: var(--text-secondary);
  font-size: 14px;
}

.agreement-note {
  margin: 26px 0 0;
  color: #91a09a;
  font-size: 12px;
  line-height: 1.7;
  text-align: center;
}

@media (max-width: 768px) {
  .auth-page {
    align-items: flex-start;
    padding: 24px 16px;
  }

  .auth-card {
    padding: 36px 28px;
  }

  .card-header h2 {
    font-size: 30px;
  }
}

@media (max-width: 480px) {
  .auth-page {
    padding: 0;
    background: #fff;
  }

  .auth-shell {
    width: 100%;
  }

  .auth-card {
    min-height: 100vh;
    padding: 40px 22px 32px;
    border: none;
    border-radius: 0;
    box-shadow: none;
  }

  .card-header {
    margin-bottom: 30px;
  }

  .card-header h2 {
    font-size: 28px;
  }

  .form-options {
    align-items: flex-start;
    font-size: 13px;
  }
}
</style>
