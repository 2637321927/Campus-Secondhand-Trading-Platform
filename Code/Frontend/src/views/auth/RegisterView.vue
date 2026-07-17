<script setup lang="ts">
import { reactive,ref } from 'vue'
import type{
    FormInstance,
    FormRules,
    } from 'element-plus'
import {ElMessage} from 'element-plus'
import {useRouter} from 'vue-router'
import {register as registerApi} from '../../api/modules/auth'
import type {RegisterRequest} from '../../types/api/auth'

interface RegisterForm{
    email:string
    userName:string
    phoneNumber:string
    password:string
    confirmPassword:string
    agreePolicy:boolean
}

const form=reactive<RegisterForm>({
    email:'',
    userName:'',
    phoneNumber:'',
    password:'',
    confirmPassword:'',
    agreePolicy:false
})

const formRef=ref<FormInstance>()
const router = useRouter()
const submitting = ref(false) //用来记录注册请求是否正在发送

const rules = reactive<FormRules<RegisterForm>>({
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

  userName: [
    {
      required: true,
      message: '请输入用户昵称',
      trigger: 'blur'
    },
    {
      min: 1,
      max: 20,
      message: '用户昵称不能超过20个字符',
      trigger: 'blur'
    }
  ],

  phoneNumber:[
    {
        pattern: /^1\d{10}$/,
        message:'请输入正确的11位手机号',
        trigger:'blur'
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
  ],

  confirmPassword: [
    {
      required: true,
      message: '请再次输入密码确认',
      trigger: 'blur'
    },
    {
      min: 6,
      message: '密码长度不能少于6位',
      trigger: 'blur'
    },
    {
        validator: (_rule, value, callback) => {
            if (value !== form.password) {
            callback(new Error('两次输入的密码不一致'))
            return
            }

            callback()
        },
        trigger: 'blur'
    }
  ],

  agreePolicy: [
    {
        validator: (_rule, value, callback) => {
            if (value !== true) {
                callback(new Error('请先阅读并同意用户协议'))
                return
            }

            callback()
            },
        trigger: 'change'
    }
  ]
})

async function handleSubmit(): Promise<void> {
  if (!formRef.value) {
    return
  }

  try{
    await formRef.value.validate()
    console.log('完整校验通过',form)
  }
  catch{
    return
  }

  const requestData:RegisterRequest={
    email:form.email.trim(),
    password:form.password,
    userName:form.userName
  }

  if(form.phoneNumber.trim()){
    requestData.phoneNumber=form.phoneNumber.trim()
  }

  submitting.value=true

  try{
    const response=await registerApi(requestData)
    ElMessage.success(response.data.message)
    await router.push('/login')
  }
  catch(error){
    console.error('注册失败',error)
    ElMessage.error('注册失败，请检查内容')
  }
  finally{
    submitting.value=false
  }
}
</script>

<template>
  <main class="register-page">
    <section class="register-shell">
      <header class="register-header">
        <p class="register-eyebrow">创建新账号</p>
        <h1>注册校园闲置</h1>
        <p class="register-subtitle">填写以下信息，开启你的校园循环交易体验。</p>
      </header>

      <el-form
        ref="formRef"
        class="register-form"
        :model="form"
        :rules="rules"
        label-position="top"
      >
        <el-form-item
          class="field-item full-row"
          label="邮箱"
          prop="email"
        >
          <el-input
            v-model="form.email"
            placeholder="请输入注册邮箱"
          />
        </el-form-item>

        <el-form-item
          class="field-item"
          label="用户昵称"
          prop="userName"
        >
          <el-input
            v-model="form.userName"
            maxlength="20"
            placeholder="请输入用户昵称"
          />
        </el-form-item>

        <el-form-item
          class="field-item"
          label="手机号（选填）"
          prop="phoneNumber"
        >
          <el-input
            v-model="form.phoneNumber"
            maxlength="11"
            placeholder="请输入11位手机号"
          />
        </el-form-item>

        <el-form-item
          class="field-item"
          label="密码"
          prop="password"
        >
          <el-input
            v-model="form.password"
            type="password"
            show-password
            placeholder="不少于6位"
          />
        </el-form-item>

        <el-form-item
          class="field-item"
          label="确认密码"
          prop="confirmPassword"
        >
          <el-input
            v-model="form.confirmPassword"
            type="password"
            show-password
            placeholder="请再次输入密码"
          />
        </el-form-item>

        <el-form-item class="agreement-row full-row" prop="agreePolicy">
          <el-checkbox v-model="form.agreePolicy">
            <span class="agreement-text">
              我已阅读并同意
              <span class="policy-link">用户协议</span>
              与
              <span class="policy-link">隐私说明</span>
            </span>
          </el-checkbox>
        </el-form-item>

        <div class="submit-row full-row">
          <el-button
            class="register-button"
            type="primary"
            :loading="submitting"
            :disabled="submitting"
            @click="handleSubmit"
          >
            注册账号
          </el-button>
        </div>

        <div class="login-entry full-row">
          <span>已有账号？</span>
          <router-link to="/login">返回登录</router-link>
        </div>
      </el-form>
    </section>
  </main>
</template>

<style scoped>
:global(*) {
  box-sizing: border-box;
}

:global(html),
:global(body),
:global(#app) {
  width: 100%;
  min-width: 0;
  min-height: 100%;
  margin: 0;
}

:global(#app) {
  max-width: none;
  padding: 0;
  text-align: initial;
}

:global(body) {
  display: block;
  min-width: 0;
  overflow-x: hidden;
  background: #ffffff;
  color: #1e2a26;
  font-family: "PingFang SC", "Microsoft YaHei", system-ui, -apple-system,
    BlinkMacSystemFont, "Segoe UI", sans-serif;
}

.register-page {
  width: 100%;
  min-height: 100vh;
  padding: 52px 24px 42px;
  background: #ffffff;
}

.register-shell {
  width: min(1120px, 100%);
  margin: 0 auto;
}

.register-header {
  margin-bottom: 46px;
  text-align: center;
}

.register-eyebrow {
  margin: 0 0 22px;
  color: #24735b;
  font-size: 18px;
  font-weight: 700;
  letter-spacing: 0.04em;
}

.register-header h1 {
  margin: 0;
  color: #1e2a26;
  font-size: clamp(42px, 5vw, 62px);
  font-weight: 500;
  line-height: 1.12;
  letter-spacing: -0.04em;
}

.register-subtitle {
  margin: 22px 0 0;
  color: #6c7a74;
  font-size: 20px;
  line-height: 1.7;
}

.register-form {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  column-gap: 36px;
  width: 100%;
}

.full-row {
  grid-column: 1 / -1;
}

.field-item {
  margin-bottom: 26px;
}

.register-form :deep(.el-form-item__label) {
  height: auto;
  padding: 0 0 13px;
  color: #1e2a26;
  font-size: 18px;
  font-weight: 700;
  line-height: 1.4;
}

.register-form :deep(.el-form-item.is-required .el-form-item__label::before) {
  color: #d9544d;
  font-size: 17px;
}

.register-form :deep(.el-input__wrapper) {
  min-height: 62px;
  padding: 0 24px;
  border-radius: 14px;
  background: #ffffff;
  box-shadow: 0 0 0 1px #dce6e1 inset;
  transition: box-shadow 0.2s ease, background-color 0.2s ease;
}

.register-form :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px #8ab2a4 inset;
}

.register-form :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 2px #24735b inset;
}

.register-form :deep(.el-input__inner) {
  height: 62px;
  color: #1e2a26;
  font-size: 18px;
}

.register-form :deep(.el-input__inner::placeholder) {
  color: #a8b2ad;
}

.register-form :deep(.el-form-item__error) {
  position: static;
  padding-top: 8px;
  color: #d9544d;
  font-size: 14px;
  line-height: 1.5;
}

.agreement-row {
  margin: 2px 0 28px;
}

.agreement-row :deep(.el-form-item__content) {
  line-height: 1.6;
}

.agreement-row :deep(.el-checkbox) {
  height: auto;
  align-items: flex-start;
  white-space: normal;
}

.agreement-row :deep(.el-checkbox__input) {
  margin-top: 4px;
}

.agreement-row :deep(.el-checkbox__inner) {
  width: 20px;
  height: 20px;
  border-radius: 6px;
}

.agreement-row :deep(.el-checkbox__label) {
  padding-left: 12px;
  color: #6c7a74;
  font-size: 17px;
  white-space: normal;
}

.agreement-text {
  color: #6c7a74;
}

.policy-link {
  color: #24735b;
  font-weight: 700;
}

.submit-row {
  width: 100%;
}

.register-button {
  width: 100%;
  height: 62px;
  border: 0;
  border-radius: 14px;
  background: #24735b;
  box-shadow: 0 14px 28px rgb(36 115 91 / 14%);
  font-size: 20px;
  font-weight: 700;
  letter-spacing: 0.02em;
}

.register-button:hover,
.register-button:focus {
  background: #3e9b79;
}

.register-button:active {
  background: #1e624e;
}

.login-entry {
  display: flex;
  justify-content: center;
  gap: 16px;
  margin-top: 36px;
  color: #6c7a74;
  font-size: 18px;
}

.login-entry a {
  color: #24735b;
  font-weight: 700;
  text-decoration: none;
}

.login-entry a:hover {
  text-decoration: underline;
  text-underline-offset: 4px;
}

@media (max-width: 819px) {
  .register-page {
    padding: 38px 20px 34px;
  }

  .register-header {
    margin-bottom: 34px;
  }

  .register-header h1 {
    font-size: clamp(38px, 9vw, 50px);
  }

  .register-subtitle {
    font-size: 17px;
  }

  .register-form {
    grid-template-columns: 1fr;
  }

  .full-row {
    grid-column: auto;
  }
}

@media (max-width: 519px) {
  .register-page {
    padding: 28px 16px 30px;
  }

  .register-eyebrow {
    margin-bottom: 14px;
    font-size: 16px;
  }

  .register-header h1 {
    font-size: 38px;
  }

  .register-subtitle {
    margin-top: 14px;
    font-size: 15px;
  }

  .register-form :deep(.el-form-item__label) {
    font-size: 16px;
  }

  .register-form :deep(.el-input__wrapper),
  .register-form :deep(.el-input__inner),
  .register-button {
    min-height: 54px;
    height: 54px;
  }

  .register-form :deep(.el-input__wrapper) {
    padding: 0 18px;
  }

  .register-form :deep(.el-input__inner) {
    font-size: 16px;
  }

  .agreement-row :deep(.el-checkbox__label),
  .login-entry {
    font-size: 15px;
  }
}
</style>