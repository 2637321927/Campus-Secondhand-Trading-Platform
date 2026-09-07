<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import type {
  FormInstance,
  FormRules,
  UploadFile
} from 'element-plus'
import {
  getMyProfile,
  updateMyProfile,
  uploadAvatar
} from '../../api/modules/user'
import type { UserProfileDto } from '../../types/api/user'
import { useAuthStore } from '../../stores/auth'
import { useAvatarImage } from '../../composables/useAvatarImage'
import { getApiErrorMessage } from '../../utils/error'

interface ProfileForm {
  userName: string
  gender: string
  phoneNumber: string
  profile: string
}

const router = useRouter()
const authStore = useAuthStore()

const profileData = ref<UserProfileDto | null>(null)
const loading = ref(false)
const saving = ref(false)
const uploadingAvatar = ref(false)
const errorMessage = ref('')

const formRef = ref<FormInstance>()
const form = reactive<ProfileForm>({
  userName: '',
  gender: 'unknown',
  phoneNumber: '',
  profile: ''
})

const { avatarUrl, loadAvatar } = useAvatarImage()

const rules: FormRules<ProfileForm> = {
  userName: [
    {
      required: true,
      message: '请输入用户名',
      trigger: 'blur'
    },
    {
      max: 20,
      message: '用户名不能超过 20 个字符',
      trigger: 'blur'
    }
  ],
  phoneNumber: [
    {
      validator: (_rule, value, callback) => {
        const text =
          typeof value === 'string' ? value.trim() : ''

        if (text !== '' && !/^\d{11}$/.test(text)) {
          callback(new Error('手机号必须为 11 位数字'))
          return
        }

        callback()
      },
      trigger: 'blur'
    }
  ],
  profile: [
    {
      max: 20,
      message: '个性签名不能超过 20 个字符',
      trigger: 'blur'
    }
  ]
}

async function loadProfileData(): Promise<void> {
  loading.value = true
  errorMessage.value = ''

  try {
    const response = await getMyProfile()

    profileData.value = response.data

    form.userName = response.data.userName
    form.gender = response.data.gender
    form.phoneNumber = response.data.phoneNumber ?? ''
    form.profile = response.data.profile ?? ''

    await loadAvatar(response.data.avatarFileId)
  } catch (error) {
    errorMessage.value = '个人资料加载失败，请稍后重试'

    console.error('个人资料加载失败：', error)
  } finally {
    loading.value = false
  }
}

async function handleAvatarChange(
  uploadFile: UploadFile
): Promise<void> {
  const rawFile = uploadFile.raw

  if (!rawFile) {
    return
  }

  uploadingAvatar.value = true

  try {
    const response = await uploadAvatar(rawFile)

    const avatarFileId = response.data.avatarFileId

    if (profileData.value) {
      profileData.value.avatarFileId = avatarFileId
    }

    if (authStore.currentUser) {
      authStore.currentUser.avatarFileId = avatarFileId
    }

    await loadAvatar(avatarFileId)

    ElMessage.success('头像更新成功')
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '头像上传失败，请稍后重试')
    )

    console.error('头像上传失败：', error)
  } finally {
    uploadingAvatar.value = false
  }
}

async function handleSave(): Promise<void> {
  if (!formRef.value) {
    return
  }

  try {
    await formRef.value.validate()
  } catch {
    return
  }

  saving.value = true

  try {
    const response = await updateMyProfile({
      userName: form.userName.trim(),
      phoneNumber: form.phoneNumber.trim() || null,
      gender: form.gender,
      profile: form.profile.trim()
    })

    profileData.value = response.data

    if (authStore.currentUser) {
      authStore.currentUser.userName = response.data.userName
      authStore.currentUser.phoneNumber = response.data.phoneNumber
      authStore.currentUser.gender = response.data.gender
      authStore.currentUser.avatarFileId = response.data.avatarFileId
    }

    ElMessage.success('资料保存成功')
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '资料保存失败，请稍后重试')
    )

    console.error('资料保存失败：', error)
  } finally {
    saving.value = false
  }
}

function goBack(): void {
  void router.push({ name: 'user-overview' })
}

onMounted(() => {
  void loadProfileData()
})
</script>

<template>
  <main class="profile-page">
    <div class="profile-container">
      <!-- 页面头部 -->
      <header class="page-header">
        <div>
          <p class="page-eyebrow">PROFILE</p>

          <h1>个人资料</h1>

          <p class="page-description">
            修改头像、昵称、性别和个性签名，完善你的个人主页信息。
          </p>
        </div>

        <el-button @click="goBack">
          返回个人中心
        </el-button>
      </header>

      <!-- 加载状态 -->
      <section
        v-if="loading"
        class="profile-panel"
      >
        <el-skeleton :rows="6" animated />
      </section>

      <!-- 错误状态 -->
      <el-result
        v-else-if="errorMessage"
        icon="error"
        title="个人资料加载失败"
        :sub-title="errorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            @click="loadProfileData"
          >
            重新加载
          </el-button>
        </template>
      </el-result>

      <!-- 资料编辑 -->
      <section
        v-else
        class="profile-panel"
      >
        <!-- 头像 -->
        <div class="avatar-section">
          <el-upload
            :show-file-list="false"
            :auto-upload="false"
            accept="image/jpeg,image/png,image/gif,image/bmp,image/webp"
            :on-change="handleAvatarChange"
          >
            <div
              class="avatar-uploader"
              :class="{ 'is-uploading': uploadingAvatar }"
            >
              <el-avatar
                class="avatar-preview"
                :size="92"
                :src="avatarUrl"
              >
                {{ form.userName?.charAt(0) ?? '用' }}
              </el-avatar>

              <span class="avatar-tip">
                {{ uploadingAvatar ? '上传中…' : '点击更换头像' }}
              </span>
            </div>
          </el-upload>

          <p class="avatar-note">
            支持 jpg / png / gif / bmp / webp 格式，大小不超过 10MB。
          </p>
        </div>

        <!-- 表单 -->
        <el-form
          ref="formRef"
          class="profile-form"
          :model="form"
          :rules="rules"
          label-position="top"
        >
          <div class="form-grid">
            <el-form-item
              label="用户名"
              prop="userName"
            >
              <el-input
                v-model="form.userName"
                maxlength="20"
                placeholder="请输入用户名"
              />
            </el-form-item>

            <el-form-item
              label="性别"
              prop="gender"
            >
              <el-select
                v-model="form.gender"
                placeholder="请选择性别"
              >
                <el-option label="男" value="male" />
                <el-option label="女" value="female" />
                <el-option label="保密" value="unknown" />
              </el-select>
            </el-form-item>

            <el-form-item
              label="手机号"
              prop="phoneNumber"
            >
              <el-input
                v-model="form.phoneNumber"
                maxlength="11"
                placeholder="选填，11 位数字"
              />
            </el-form-item>

            <el-form-item label="邮箱（不可修改）">
              <el-input
                :model-value="profileData?.email ?? ''"
                disabled
              />
            </el-form-item>
          </div>

          <el-form-item
            label="个性签名"
            prop="profile"
          >
            <el-input
              v-model="form.profile"
              type="textarea"
              :rows="3"
              maxlength="20"
              show-word-limit
              placeholder="介绍一下自己（20 字以内）"
            />
          </el-form-item>

          <div class="form-actions">
            <el-button
              type="primary"
              :loading="saving"
              :disabled="saving || uploadingAvatar"
              @click="handleSave"
            >
              保存修改
            </el-button>

            <el-button
              :disabled="saving || uploadingAvatar"
              @click="goBack"
            >
              取消
            </el-button>
          </div>
        </el-form>
      </section>
    </div>
  </main>
</template>

<style scoped>
.profile-page {
  min-height: calc(100vh - 72px);
  padding: 36px 24px 64px;
  background: #f5f7f6;
  color: #1e2a26;
}

.profile-container {
  width: 100%;
  max-width: 860px;
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

.profile-panel {
  padding: 30px 32px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

/* 头像区域 */
.avatar-section {
  display: flex;
  align-items: center;
  gap: 22px;
  padding-bottom: 26px;
  margin-bottom: 26px;
  border-bottom: 1px solid #edf1ef;
}

.avatar-uploader {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.avatar-uploader.is-uploading {
  pointer-events: none;
  opacity: 0.6;
}

.avatar-preview {
  color: #ffffff;
  background: #3e9b79;
  font-size: 32px;
  font-weight: 700;
}

.avatar-tip {
  color: #3e9b79;
  font-size: 13px;
  font-weight: 600;
}

.avatar-note {
  margin: 0;
  color: #7a8781;
  font-size: 13px;
  line-height: 1.7;
}

/* 表单 */
.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0 20px;
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

  .form-grid {
    grid-template-columns: 1fr;
  }

  .avatar-section {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
