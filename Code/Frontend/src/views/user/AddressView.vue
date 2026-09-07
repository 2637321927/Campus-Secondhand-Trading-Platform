<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  ElMessage,
  ElMessageBox
} from 'element-plus'
import type {
  FormInstance,
  FormRules
} from 'element-plus'
import {
  getMyAddresses,
  createAddress,
  updateAddress,
  deleteAddress,
  setDefaultAddress
} from '../../api/modules/address'
import type {
  AddressDto,
  CreateAddressRequest,
  UpdateAddressRequest
} from '../../types/api/address'
import { getApiErrorMessage } from '../../utils/error'

type DialogMode = 'create' | 'edit'

interface AddressForm {
  name: string
  phoneNumber: string
  detailAddress: string
  isDefault: boolean
}

const router = useRouter()

const addresses = ref<AddressDto[]>([])
const loading = ref(false)
const errorMessage = ref('')
const hasLoaded = ref(false)

const dialogVisible = ref(false)
const dialogMode = ref<DialogMode>('create')
const editingAddressId = ref<number | null>(null)
const saving = ref(false)

const deletingAddressIds = ref<number[]>([])
const settingDefaultId = ref<number | null>(null)

const formRef = ref<FormInstance>()
const form = reactive<AddressForm>({
  name: '',
  phoneNumber: '',
  detailAddress: '',
  isDefault: false
})

const dialogTitle = () =>
  dialogMode.value === 'create'
    ? '新增地址'
    : '编辑地址'

const rules: FormRules<AddressForm> = {
  name: [
    {
      required: true,
      message: '请输入收货人姓名',
      trigger: 'blur'
    },
    {
      max: 10,
      message: '收货人姓名不能超过 10 个字符',
      trigger: 'blur'
    }
  ],
  phoneNumber: [
    {
      required: true,
      message: '请输入手机号',
      trigger: 'blur'
    },
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
  detailAddress: [
    {
      required: true,
      message: '请输入详细地址',
      trigger: 'blur'
    },
    {
      max: 50,
      message: '详细地址不能超过 50 个字符',
      trigger: 'blur'
    }
  ]
}

function resetForm(): void {
  form.name = ''
  form.phoneNumber = ''
  form.detailAddress = ''
  form.isDefault = false

  formRef.value?.clearValidate()
}

async function loadAddresses(): Promise<void> {
  loading.value = true
  errorMessage.value = ''

  try {
    const response = await getMyAddresses()

    addresses.value = response.data ?? []
  } catch (error) {
    errorMessage.value = '地址列表加载失败，请稍后重试'

    console.error('地址列表加载失败：', error)
  } finally {
    loading.value = false
    hasLoaded.value = true
  }
}

function openCreateDialog(): void {
  dialogMode.value = 'create'
  editingAddressId.value = null
  resetForm()
  dialogVisible.value = true
}

function openEditDialog(address: AddressDto): void {
  dialogMode.value = 'edit'
  editingAddressId.value = address.addressId

  form.name = address.name
  form.phoneNumber = address.phoneNumber
  form.detailAddress = address.detailAddress
  form.isDefault = address.isDefault

  dialogVisible.value = true
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
    if (dialogMode.value === 'create') {
      const requestData: CreateAddressRequest = {
        name: form.name.trim(),
        phoneNumber: form.phoneNumber.trim(),
        detailAddress: form.detailAddress.trim(),
        isDefault: form.isDefault
      }

      await createAddress(requestData)

      ElMessage.success('地址新增成功')
    } else {
      const requestData: UpdateAddressRequest = {
        name: form.name.trim(),
        phoneNumber: form.phoneNumber.trim(),
        detailAddress: form.detailAddress.trim()
      }

      await updateAddress(
        editingAddressId.value as number,
        requestData
      )

      ElMessage.success('地址修改成功')
    }

    dialogVisible.value = false

    await loadAddresses()
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '地址保存失败，请稍后重试')
    )

    console.error('地址保存失败：', error)
  } finally {
    saving.value = false
  }
}

function isDeleting(addressId: number): boolean {
  return deletingAddressIds.value.includes(addressId)
}

async function handleDelete(address: AddressDto): Promise<void> {
  if (isDeleting(address.addressId)) {
    return
  }

  try {
    await ElMessageBox.confirm(
      `确定删除收货人「${address.name}」的地址吗？`,
      '删除地址',
      {
        type: 'warning',
        confirmButtonText: '删除',
        cancelButtonText: '取消'
      }
    )
  } catch {
    return
  }

  deletingAddressIds.value = [
    ...deletingAddressIds.value,
    address.addressId
  ]

  try {
    await deleteAddress(address.addressId)

    ElMessage.success('地址已删除')

    await loadAddresses()
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '删除失败，请稍后重试')
    )

    console.error('删除地址失败：', error)
  } finally {
    deletingAddressIds.value =
      deletingAddressIds.value.filter(
        (id) => id !== address.addressId
      )
  }
}

async function handleSetDefault(
  address: AddressDto
): Promise<void> {
  if (
    address.isDefault ||
    settingDefaultId.value !== null
  ) {
    return
  }

  settingDefaultId.value = address.addressId

  try {
    await setDefaultAddress(address.addressId)

    ElMessage.success('已设为默认地址')

    await loadAddresses()
  } catch (error) {
    ElMessage.error(
      getApiErrorMessage(error, '设置默认地址失败，请稍后重试')
    )

    console.error('设置默认地址失败：', error)
  } finally {
    settingDefaultId.value = null
  }
}

function goBack(): void {
  void router.push({ name: 'user-overview' })
}

onMounted(() => {
  void loadAddresses()
})
</script>

<template>
  <main class="address-page">
    <div class="address-container">
      <!-- 页面头部 -->
      <header class="page-header">
        <div>
          <p class="page-eyebrow">ADDRESS</p>

          <h1>地址管理</h1>

          <p class="page-description">
            管理你的收货与交易地址，默认地址将优先用于下单和配送。
          </p>
        </div>

        <div class="header-actions">
          <el-button @click="goBack">
            返回个人中心
          </el-button>

          <el-button
            type="primary"
            @click="openCreateDialog"
          >
            新增地址
          </el-button>
        </div>
      </header>

      <!-- 加载状态 -->
      <section
        v-if="loading && !hasLoaded"
        class="address-panel"
      >
        <div
          v-for="index in 3"
          :key="index"
          class="address-skeleton"
        >
          <el-skeleton animated>
            <template #template>
              <div class="skeleton-row">
                <el-skeleton-item
                  variant="h3"
                  class="skeleton-title"
                />

                <el-skeleton-item
                  variant="text"
                  class="skeleton-text"
                />
              </div>
            </template>
          </el-skeleton>
        </div>
      </section>

      <!-- 错误状态 -->
      <el-result
        v-else-if="errorMessage && addresses.length === 0"
        icon="error"
        title="地址列表加载失败"
        :sub-title="errorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            @click="loadAddresses"
          >
            重新加载
          </el-button>
        </template>
      </el-result>

      <!-- 空状态 -->
      <el-empty
        v-else-if="hasLoaded && addresses.length === 0"
        description="还没有添加收货地址"
        class="address-empty"
      >
        <el-button
          type="primary"
          plain
          @click="openCreateDialog"
        >
          添加第一个地址
        </el-button>
      </el-empty>

      <!-- 地址列表 -->
      <section
        v-else
        class="address-panel"
        v-loading="loading"
      >
        <div class="panel-header">
          <div>
            <h2>我的地址</h2>

            <span>共 {{ addresses.length }} 条地址</span>
          </div>
        </div>

        <ul class="address-list">
          <li
            v-for="address in addresses"
            :key="address.addressId"
            class="address-item"
          >
            <div class="address-main">
              <div class="address-heading">
                <h3>{{ address.name }}</h3>

                <el-tag
                  v-if="address.isDefault"
                  type="success"
                  effect="light"
                  size="small"
                >
                  默认地址
                </el-tag>
              </div>

              <p class="address-phone">
                {{ address.phoneNumber }}
              </p>

              <p class="address-detail">
                {{ address.detailAddress }}
              </p>
            </div>

            <div class="address-actions">
              <el-button
                v-if="!address.isDefault"
                type="primary"
                plain
                :loading="
                  settingDefaultId === address.addressId
                "
                :disabled="settingDefaultId !== null"
                @click="handleSetDefault(address)"
              >
                设为默认
              </el-button>

              <el-button @click="openEditDialog(address)">
                编辑
              </el-button>

              <el-button
                type="danger"
                link
                :loading="isDeleting(address.addressId)"
                :disabled="deletingAddressIds.length > 0"
                @click="handleDelete(address)"
              >
                删除
              </el-button>
            </div>
          </li>
        </ul>
      </section>
    </div>

    <!-- 新增 / 编辑地址弹窗 -->
    <el-dialog
      v-model="dialogVisible"
      :title="dialogTitle()"
      width="480px"
      destroy-on-close
      :close-on-click-modal="false"
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-position="top"
      >
        <el-form-item
          label="收货人姓名"
          prop="name"
        >
          <el-input
            v-model="form.name"
            maxlength="10"
            placeholder="请输入收货人姓名"
          />
        </el-form-item>

        <el-form-item
          label="手机号"
          prop="phoneNumber"
        >
          <el-input
            v-model="form.phoneNumber"
            maxlength="11"
            placeholder="请输入 11 位手机号"
          />
        </el-form-item>

        <el-form-item
          label="详细地址"
          prop="detailAddress"
        >
          <el-input
            v-model="form.detailAddress"
            type="textarea"
            :rows="3"
            maxlength="50"
            show-word-limit
            placeholder="请输入详细地址，如宿舍楼、门牌号等"
          />
        </el-form-item>

        <el-form-item
          v-if="dialogMode === 'create'"
          label="设为默认地址"
        >
          <el-switch
            v-model="form.isDefault"
            active-text="是"
            inactive-text="否"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button
          :disabled="saving"
          @click="dialogVisible = false"
        >
          取消
        </el-button>

        <el-button
          type="primary"
          :loading="saving"
          :disabled="saving"
          @click="handleSave"
        >
          保存
        </el-button>
      </template>
    </el-dialog>
  </main>
</template>

<style scoped>
.address-page {
  min-height: calc(100vh - 72px);
  padding: 36px 24px 64px;
  background: #f5f7f6;
  color: #1e2a26;
}

.address-container {
  width: 100%;
  max-width: 900px;
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

.header-actions {
  display: flex;
  flex-shrink: 0;
  gap: 12px;
}

.address-panel {
  padding: 26px 28px;
  background: #ffffff;
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.panel-header {
  display: flex;
  padding-bottom: 18px;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #edf1ef;
}

.panel-header h2 {
  margin: 0;
  color: #1e2a26;
  font-size: 20px;
}

.panel-header span {
  display: block;
  margin-top: 6px;
  color: #7a8781;
  font-size: 13px;
}

.address-list {
  margin: 0;
  padding: 0;
  list-style: none;
}

.address-item {
  display: flex;
  padding: 20px 0;
  align-items: center;
  gap: 18px;
  border-bottom: 1px solid #edf1ef;
}

.address-item:last-child {
  border-bottom: 0;
}

.address-main {
  min-width: 0;
  flex: 1;
}

.address-heading {
  display: flex;
  align-items: center;
  gap: 10px;
}

.address-heading h3 {
  margin: 0;
  color: #1e2a26;
  font-size: 17px;
}

.address-phone {
  margin: 8px 0 0;
  color: #6c7a74;
  font-size: 14px;
}

.address-detail {
  margin: 6px 0 0;
  color: #46534d;
  font-size: 14px;
  line-height: 1.7;
  overflow-wrap: anywhere;
}

.address-actions {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  gap: 8px;
}

.address-skeleton {
  padding: 8px 0;
}

.skeleton-row {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.skeleton-title {
  width: 30%;
  height: 20px;
}

.skeleton-text {
  width: 60%;
  height: 14px;
}

.address-empty {
  padding: 70px 0 55px;
}

@media (max-width: 640px) {
  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .address-item {
    flex-wrap: wrap;
  }

  .address-actions {
    width: 100%;
    justify-content: flex-end;
  }
}
</style>
