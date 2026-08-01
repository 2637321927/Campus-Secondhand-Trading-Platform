<script setup lang="ts">
import {
  computed,
  nextTick,
  onBeforeUnmount,
  onMounted,
  reactive,
  ref,
  watch
} from 'vue'
import {
  onBeforeRouteLeave,
  onBeforeRouteUpdate,
  useRoute,
  useRouter
} from 'vue-router'
import axios from 'axios'
import {
  ElMessage,
  ElMessageBox
} from 'element-plus'
import type {
  FormInstance,
  FormRules
} from 'element-plus'
import { getCategories } from '../../api/modules/category'
import { getSellerProductDetail } from '../../api/modules/seller'
import {
  sortProductImages,
  updateProduct
} from '../../api/modules/product'
import type {
  CategoryDto
} from '../../types/api/category'
import type {
  ProductDto,
  ProductImageDto,
  ProductStatus,
  UpdateProductRequest
} from '../../types/api/product'
import { resolveFileUrl } from '../../utils/image'

interface ProductEditForm {
  name: string
  price: number | null
  categoryId: number | null
  info: string
  status: ProductStatus
  shippingMethodId: number | null
}

interface NewImagePreview {
  id: string
  file: File
  url: string
}

const MAX_IMAGE_COUNT = 9
const MAX_IMAGE_SIZE = 5 * 1024 * 1024

const route = useRoute()
const router = useRouter()

const formRef = ref<FormInstance>()
const imageInputRef = ref<HTMLInputElement>()

const form = reactive<ProductEditForm>({
  name: '',
  price: null,
  categoryId: null,
  info: '',
  status: 0,
  shippingMethodId: null
})

const categories = ref<CategoryDto[]>([])
const categoriesLoading = ref(false)
const categoriesErrorMessage = ref('')

const existingImages = ref<ProductImageDto[]>([])
const newImagePreviews = ref<NewImagePreview[]>([])
const failedNewImageIds = ref<string[]>([])
const toRemoveImageIds = ref<number[]>([])
const existingOrderChanged = ref(false)

const loadedAddressId =
  ref<number | null | undefined>(undefined)

const loading = ref(false)
const errorMessage = ref('')
const saving = ref(false)

const hasUnsavedChanges = ref(false)
let dirtyTrackingEnabled = false
let allowLeaveWithoutConfirm = false
let loadVersion = 0

const productId = computed<number | null>(() => {
  const value = route.params.productId

  if (typeof value !== 'string') {
    return null
  }

  const id = Number(value)

  if (!Number.isInteger(id) || id <= 0) {
    return null
  }

  return id
})

const totalImageCount = computed(() => {
  return (
    existingImages.value.length +
    newImagePreviews.value.length
  )
})

const canManageExistingImages = computed(() => {
  if (existingImages.value.length === 0) {
    return true
  }

  return existingImages.value.every((image) => {
    return (
      Number.isInteger(image.imageId) &&
      (image.imageId ?? 0) > 0
    )
  })
})

const rules: FormRules<ProductEditForm> = {
  name: [
    {
      required: true,
      validator: validateProductName,
      trigger: 'blur'
    },
    {
      max: 30,
      message: '商品名称不能超过 30 个字符',
      trigger: 'blur'
    }
  ],
  price: [
    {
      required: true,
      message: '请输入商品价格',
      trigger: 'change'
    },
    {
      type: 'number',
      min: 0.01,
      message: '商品价格必须大于 0',
      trigger: 'change'
    }
  ],
  categoryId: [
    {
      required: true,
      message: '请选择商品分类',
      trigger: 'change'
    }
  ],
  info: [
    {
      max: 100,
      message: '商品描述不能超过 100 个字符',
      trigger: 'blur'
    }
  ]
}

function validateProductName(
  _rule: unknown,
  value: string,
  callback: (error?: Error) => void
): void {
  if (!value.trim()) {
    callback(new Error('请输入商品名称'))
    return
  }

  callback()
}

function markAsChanged(): void {
  if (!dirtyTrackingEnabled) {
    return
  }

  hasUnsavedChanges.value = true
}

function clearNewImagePreviews(): void {
  for (const preview of newImagePreviews.value) {
    URL.revokeObjectURL(preview.url)
  }

  newImagePreviews.value = []
  failedNewImageIds.value = []
}

function resetImageEditor(): void {
  clearNewImagePreviews()
  existingImages.value = []
  toRemoveImageIds.value = []
  existingOrderChanged.value = false
}

function applyProduct(product: ProductDto): void {
  dirtyTrackingEnabled = false

  form.name = product.name
  form.price = product.price
  form.categoryId = product.categoryId
  form.info = product.info ?? ''
  form.status = product.status
  form.shippingMethodId =
    product.shippingMethodId ?? null

  loadedAddressId.value = product.addressId

  clearNewImagePreviews()
  existingImages.value = [
    ...(product.images ?? [])
  ].sort((a, b) => {
    return a.imgIndex - b.imgIndex
  })

  toRemoveImageIds.value = []
  existingOrderChanged.value = false
  hasUnsavedChanges.value = false

  void nextTick(() => {
    dirtyTrackingEnabled = true
  })
}

function getLoadErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return '商品编辑信息加载失败，请稍后重试'
  }

  if (!error.response) {
    return '无法连接后端服务，请稍后重试'
  }

  if (error.response.status === 403) {
    return '你无权编辑这件商品'
  }

  if (error.response.status === 404) {
    return '商品不存在，或卖家详情接口尚未实现'
  }

  return '商品编辑信息加载失败，请稍后重试'
}

async function loadProduct(): Promise<void> {
  const requestedProductId = productId.value
  const currentVersion = ++loadVersion

  allowLeaveWithoutConfirm = false
  dirtyTrackingEnabled = false
  hasUnsavedChanges.value = false
  errorMessage.value = ''
  loading.value = true
  resetImageEditor()

  if (requestedProductId === null) {
    errorMessage.value = '商品编号不正确'
    loading.value = false
    return
  }

  try {
    const response = await getSellerProductDetail(
      requestedProductId
    )

    if (
      currentVersion !== loadVersion ||
      productId.value !== requestedProductId
    ) {
      return
    }

    applyProduct(response.data)
  } catch (error) {
    if (currentVersion !== loadVersion) {
      return
    }

    errorMessage.value = getLoadErrorMessage(error)
    console.error('商品编辑信息加载失败：', error)
  } finally {
    if (currentVersion === loadVersion) {
      loading.value = false
    }
  }
}

async function loadCategories(): Promise<void> {
  categoriesLoading.value = true
  categoriesErrorMessage.value = ''

  try {
    const response = await getCategories()
    categories.value = response.data ?? []
  } catch (error) {
    categoriesErrorMessage.value =
      '商品分类加载失败，请稍后重试'

    console.error('商品分类加载失败：', error)
  } finally {
    categoriesLoading.value = false
  }
}

function openImageSelector(): void {
  if (saving.value) {
    return
  }

  if (totalImageCount.value >= MAX_IMAGE_COUNT) {
    ElMessage.warning(
      `商品图片不能超过 ${MAX_IMAGE_COUNT} 张`
    )
    return
  }

  imageInputRef.value?.click()
}

function handleImageChange(event: Event): void {
  const input = event.target as HTMLInputElement
  const selectedFiles = Array.from(input.files ?? [])

  input.value = ''

  if (saving.value) {
    return
  }

  if (selectedFiles.length === 0) {
    return
  }

  const remainingCount =
    MAX_IMAGE_COUNT - totalImageCount.value
  const validFiles: File[] = []

  for (const file of selectedFiles) {
    if (!file.type.startsWith('image/')) {
      ElMessage.warning(
        `${file.name} 不是有效的图片文件`
      )
      continue
    }

    if (file.size > MAX_IMAGE_SIZE) {
      ElMessage.warning(
        `${file.name} 超过 5 MB，无法添加`
      )
      continue
    }

    const duplicatedInExistingSelection =
      newImagePreviews.value.some((preview) => {
        return (
          preview.file.name === file.name &&
          preview.file.size === file.size &&
          preview.file.lastModified === file.lastModified
        )
      })

    const duplicatedInCurrentSelection =
      validFiles.some((selectedFile) => {
        return (
          selectedFile.name === file.name &&
          selectedFile.size === file.size &&
          selectedFile.lastModified === file.lastModified
        )
      })

    if (
      duplicatedInExistingSelection ||
      duplicatedInCurrentSelection
    ) {
      ElMessage.warning(
        `${file.name} 已经选择，请勿重复添加`
      )
      continue
    }

    validFiles.push(file)
  }

  const filesToAdd = validFiles.slice(
    0,
    remainingCount
  )

  if (validFiles.length > remainingCount) {
    ElMessage.warning(
      `商品图片不能超过 ${MAX_IMAGE_COUNT} 张`
    )
  }

  for (const file of filesToAdd) {
    newImagePreviews.value.push({
      id: `${file.name}-${file.size}-${file.lastModified}`,
      file,
      url: URL.createObjectURL(file)
    })
  }

  if (filesToAdd.length > 0) {
    markAsChanged()
  }
}

function removeNewImage(index: number): void {
  if (saving.value) {
    return
  }

  const preview = newImagePreviews.value[index]

  if (preview) {
    URL.revokeObjectURL(preview.url)
    failedNewImageIds.value =
      failedNewImageIds.value.filter(
        (id) => id !== preview.id
      )
  }

  newImagePreviews.value.splice(index, 1)
  markAsChanged()
}

function handleNewImagePreviewError(
  previewId: string
): void {
  if (failedNewImageIds.value.includes(previewId)) {
    return
  }

  failedNewImageIds.value.push(previewId)
  markAsChanged()

  ElMessage.error(
    '有新图片无法正常预览，请删除后重新选择'
  )
}

function moveNewImage(
  currentIndex: number,
  targetIndex: number
): void {
  if (saving.value) {
    return
  }

  if (
    targetIndex < 0 ||
    targetIndex >= newImagePreviews.value.length
  ) {
    return
  }

  const [preview] = newImagePreviews.value.splice(
    currentIndex,
    1
  )

  if (!preview) {
    return
  }

  newImagePreviews.value.splice(
    targetIndex,
    0,
    preview
  )

  markAsChanged()
}

function removeExistingImage(index: number): void {
  if (saving.value) {
    return
  }

  const image = existingImages.value[index]

  if (
    !image ||
    !Number.isInteger(image.imageId) ||
    (image.imageId ?? 0) <= 0
  ) {
    ElMessage.warning(
      '后端未返回 imageId，不能安全删除该图片'
    )
    return
  }

  toRemoveImageIds.value.push(image.imageId as number)
  existingImages.value.splice(index, 1)
  existingOrderChanged.value = true
  markAsChanged()
}

function moveExistingImage(
  currentIndex: number,
  targetIndex: number
): void {
  if (saving.value) {
    return
  }

  if (!canManageExistingImages.value) {
    ElMessage.warning(
      '后端未返回完整 imageId，已有图片排序已禁用'
    )
    return
  }

  if (
    targetIndex < 0 ||
    targetIndex >= existingImages.value.length
  ) {
    return
  }

  const [image] = existingImages.value.splice(
    currentIndex,
    1
  )

  if (!image) {
    return
  }

  existingImages.value.splice(targetIndex, 0, image)
  existingOrderChanged.value = true
  markAsChanged()
}

function createUpdateRequest(): UpdateProductRequest | null {
  if (
    form.price === null ||
    form.categoryId === null
  ) {
    return null
  }

  const requestData: UpdateProductRequest = {
    name: form.name.trim(),
    price: form.price,
    info: form.info.trim(),
    categoryId: form.categoryId,
    status: form.status,
    newImages: newImagePreviews.value.map(
      (preview) => preview.file
    ),
    toRemoveImageIds: [
      ...toRemoveImageIds.value
    ]
  }

  if (form.shippingMethodId !== null) {
    requestData.shippingMethodId =
      form.shippingMethodId
  }

  if (
    loadedAddressId.value !== null &&
    loadedAddressId.value !== undefined
  ) {
    requestData.addressId =
      loadedAddressId.value
  }

  return requestData
}

function productMatchesRequest(
  product: ProductDto,
  requestData: UpdateProductRequest,
  expectedImageCount: number
): boolean {
  const shippingMethodMatches =
    requestData.shippingMethodId === undefined ||
    product.shippingMethodId ===
      requestData.shippingMethodId

  const addressMatches =
    requestData.addressId === undefined ||
    product.addressId === requestData.addressId

  return (
    product.name === requestData.name &&
    Number(product.price) === requestData.price &&
    product.categoryId === requestData.categoryId &&
    (product.info ?? '') === (requestData.info ?? '') &&
    product.status === requestData.status &&
    shippingMethodMatches &&
    addressMatches &&
    (product.images ?? []).length === expectedImageCount
  )
}

function getExistingImageOrder(): number[] | null {
  const imageIds: number[] = []

  for (const image of existingImages.value) {
    if (
      !Number.isInteger(image.imageId) ||
      (image.imageId ?? 0) <= 0
    ) {
      return null
    }

    imageIds.push(image.imageId as number)
  }

  return imageIds
}

async function applyImageOrderIfPossible(
  requestedProductId: number,
  refreshedProduct: ProductDto,
  requestedExistingOrder: number[] | null
): Promise<ProductDto> {
  if (
    !existingOrderChanged.value ||
    requestedExistingOrder === null
  ) {
    return refreshedProduct
  }

  const refreshedImages = [
    ...(refreshedProduct.images ?? [])
  ].sort((a, b) => {
    return a.imgIndex - b.imgIndex
  })

  const allImageIdsAvailable =
    refreshedImages.every((image) => {
      return (
        Number.isInteger(image.imageId) &&
        (image.imageId ?? 0) > 0
      )
    })

  if (!allImageIdsAvailable) {
    ElMessage.warning(
      '商品已保存，但后端未返回 imageId，无法提交图片排序'
    )
    return refreshedProduct
  }

  const existingIdSet = new Set(
    requestedExistingOrder
  )

  const newImageIds = refreshedImages
    .map((image) => image.imageId as number)
    .filter((imageId) => {
      return !existingIdSet.has(imageId)
    })

  const refreshedIdSet = new Set(
    refreshedImages.map(
      (image) => image.imageId as number
    )
  )

  const imageIds = requestedExistingOrder.filter(
    (imageId) => refreshedIdSet.has(imageId)
  )

  imageIds.push(...newImageIds)

  if (imageIds.length !== refreshedImages.length) {
    ElMessage.warning(
      '商品已保存，但图片标识发生变化，未提交排序'
    )
    return refreshedProduct
  }

  await sortProductImages(
    requestedProductId,
    {
      imageIds
    }
  )

  const finalResponse = await getSellerProductDetail(
    requestedProductId
  )

  return finalResponse.data
}

async function saveProduct(): Promise<void> {
  if (saving.value || !formRef.value) {
    return
  }

  if (failedNewImageIds.value.length > 0) {
    ElMessage.warning(
      '存在无法预览的新图片，请删除后重新选择'
    )
    return
  }

  try {
    await formRef.value.validate()
  } catch {
    ElMessage.warning('请先完善商品信息')
    return
  }

  const requestedProductId = productId.value
  const requestData = createUpdateRequest()

  if (
    requestedProductId === null ||
    requestData === null
  ) {
    return
  }

  const requestedExistingOrder =
    getExistingImageOrder()
  const expectedImageCount = totalImageCount.value

  saving.value = true
  let updateSucceeded = false

  try {
    const updateResponse = await updateProduct(
      requestedProductId,
      requestData
    )

    updateSucceeded = true

    let refreshedProduct: ProductDto

    try {
      const refreshedResponse =
        await getSellerProductDetail(
          requestedProductId
        )

      refreshedProduct = refreshedResponse.data
    } catch (error) {
      console.error('保存后重新读取商品失败：', error)

      applyProduct(updateResponse.data)

      ElMessage.warning(
        '保存请求已成功，但重新读取验证失败，请稍后刷新确认'
      )
      return
    }

    let imageOrderFailed = false

    try {
      refreshedProduct =
        await applyImageOrderIfPossible(
          requestedProductId,
          refreshedProduct,
          requestedExistingOrder
        )
    } catch (error) {
      imageOrderFailed = true

      console.error(
        '商品已保存，但图片排序或排序验证失败：',
        error
      )
    }

    const persisted =
      productMatchesRequest(
        refreshedProduct,
        requestData,
        expectedImageCount
      )

    applyProduct(refreshedProduct)

    if (imageOrderFailed) {
      ElMessage.warning(
        '商品内容已保存，但图片排序未完成，请稍后刷新确认'
      )
    } else if (persisted) {
      ElMessage.success('商品修改已保存')
    } else {
      ElMessage.warning(
        '保存请求成功，但重新读取的数据与提交内容不完全一致'
      )
    }
  } catch (error) {
    if (updateSucceeded) {
      console.error(
        '商品已保存，但页面处理保存结果失败：',
        error
      )

      ElMessage.warning(
        '保存请求已成功，但结果处理失败，请刷新页面确认'
      )
      return
    }

    console.error('保存商品失败：', error)
    ElMessage.error(
      '商品保存失败，当前表单内容已保留'
    )
  } finally {
    saving.value = false
  }
}

async function goBack(): Promise<void> {
  if (productId.value === null) {
    await router.push({
      name: 'my-products'
    })
    return
  }

  await router.push({
    name: 'seller-product-detail',
    params: {
      productId: productId.value
    }
  })
}

function handleBeforeUnload(
  event: BeforeUnloadEvent
): void {
  if (
    allowLeaveWithoutConfirm ||
    !hasUnsavedChanges.value
  ) {
    return
  }

  event.preventDefault()
  event.returnValue = ''
}

async function confirmLeaveEditPage(): Promise<boolean> {
  if (saving.value) {
    ElMessage.warning('商品正在保存，请稍候')
    return false
  }

  if (
    allowLeaveWithoutConfirm ||
    !hasUnsavedChanges.value
  ) {
    return true
  }

  try {
    await ElMessageBox.confirm(
      '当前修改尚未保存，确定离开编辑页吗？',
      '未保存的修改',
      {
        confirmButtonText: '确认离开',
        cancelButtonText: '继续编辑',
        type: 'warning'
      }
    )

    allowLeaveWithoutConfirm = true
    return true
  } catch {
    return false
  }
}

watch(
  [
    () => form.name,
    () => form.price,
    () => form.categoryId,
    () => form.info,
    () => form.status,
    () => form.shippingMethodId
  ],
  () => {
    markAsChanged()
  }
)

watch(
  productId,
  () => {
    void loadProduct()
  },
  {
    immediate: true
  }
)

onMounted(() => {
  void loadCategories()

  window.addEventListener(
    'beforeunload',
    handleBeforeUnload
  )
})

onBeforeUnmount(() => {
  loadVersion += 1

  window.removeEventListener(
    'beforeunload',
    handleBeforeUnload
  )

  clearNewImagePreviews()
})

onBeforeRouteUpdate(async (to) => {
  if (
    to.params.productId ===
    route.params.productId
  ) {
    return true
  }

  return confirmLeaveEditPage()
})

onBeforeRouteLeave(async () => {
  return confirmLeaveEditPage()
})
</script>

<template>
  <main class="product-edit-page">
    <section class="product-edit-container">
      <header class="page-header">
        <div>
          <h1>编辑商品</h1>
          <p>修改商品资料并保存后端返回的最新结果。</p>
        </div>

        <el-button
          :disabled="saving"
          @click="goBack"
        >
          返回管理详情
        </el-button>
      </header>

      <div
        v-if="loading"
        class="page-state"
      >
        <el-skeleton :rows="9" animated />
      </div>

      <el-result
        v-else-if="errorMessage"
        icon="error"
        title="无法加载商品编辑信息"
        :sub-title="errorMessage"
      >
        <template #extra>
          <el-button
            type="primary"
            @click="loadProduct"
          >
            重新加载
          </el-button>

          <el-button @click="goBack">
            返回
          </el-button>
        </template>
      </el-result>

      <el-form
        v-else
        ref="formRef"
        :model="form"
        :rules="rules"
        :disabled="saving"
        label-position="top"
        class="edit-form"
      >
        <el-card
          class="form-card"
          shadow="never"
        >
          <template #header>
            <h2>商品信息</h2>
          </template>

          <div class="form-grid">
            <el-form-item
              label="商品名称"
              prop="name"
            >
              <el-input
                v-model="form.name"
                maxlength="30"
                show-word-limit
                clearable
              />
            </el-form-item>

            <el-form-item
              label="商品价格"
              prop="price"
            >
              <el-input-number
                v-model="form.price"
                :min="0.01"
                :precision="2"
                :step="1"
                controls-position="right"
                class="full-control"
              />
            </el-form-item>

            <el-form-item
              label="商品分类"
              prop="categoryId"
            >
              <el-select
                v-model="form.categoryId"
                filterable
                :loading="categoriesLoading"
                class="full-control"
              >
                <el-option
                  v-for="category in categories"
                  :key="category.categoryId"
                  :label="category.categoryName"
                  :value="category.categoryId"
                />
              </el-select>

              <div
                v-if="categoriesErrorMessage"
                class="field-error"
              >
                {{ categoriesErrorMessage }}

                <el-button
                  link
                  type="primary"
                  @click="loadCategories"
                >
                  重试
                </el-button>
              </div>
            </el-form-item>

            <el-form-item
              label="商品状态"
              prop="status"
            >
              <el-select
                v-model="form.status"
                class="full-control"
              >
                <el-option label="在售" :value="0" />
                <el-option label="已售" :value="1" />
                <el-option label="已下架" :value="2" />
                <el-option
                  label="草稿（待后端确认）"
                  :value="3"
                />
              </el-select>
            </el-form-item>

            <el-form-item
              label="交易方式"
              prop="shippingMethodId"
            >
              <el-select
                v-model="form.shippingMethodId"
                clearable
                placeholder="后端未返回时可不选择"
                class="full-control"
              >
                <el-option label="当面交易" :value="1" />
                <el-option label="快递交易" :value="2" />
                <el-option label="两者均可" :value="3" />
              </el-select>
            </el-form-item>
          </div>

          <el-form-item
            label="商品描述"
            prop="info"
          >
            <el-input
              v-model="form.info"
              type="textarea"
              :rows="5"
              maxlength="100"
              show-word-limit
              placeholder="可以留空；保存空字符串会清除原有描述"
            />
          </el-form-item>

          <el-alert
            title="地址接口尚未明确"
            type="info"
            :closable="false"
            show-icon
          >
            当前页面不会虚构地址列表。若详情响应包含 addressId，
            保存时会原样保留；否则不会发送地址字段。
          </el-alert>
        </el-card>

        <el-card
          class="form-card"
          shadow="never"
        >
          <template #header>
            <div class="image-section-header">
              <div>
                <h2>商品图片</h2>
                <p>
                  共 {{ totalImageCount }} /
                  {{ MAX_IMAGE_COUNT }} 张，第一张为封面。
                </p>
              </div>

              <el-button
                type="primary"
                plain
                :disabled="
                  saving ||
                  totalImageCount >= MAX_IMAGE_COUNT
                "
                @click="openImageSelector"
              >
                选择新图片
              </el-button>
            </div>
          </template>

          <input
            ref="imageInputRef"
            type="file"
            accept="image/*"
            multiple
            :disabled="saving"
            class="file-input"
            @change="handleImageChange"
          >

          <el-alert
            v-if="
              existingImages.length > 0 &&
              !canManageExistingImages
            "
            title="后端未返回 imageId"
            type="warning"
            :closable="false"
            show-icon
            class="image-alert"
          >
            为避免把 imgFileId 当成 imageId，
            已有图片的删除和排序操作已禁用。
          </el-alert>

          <section v-if="existingImages.length > 0">
            <h3>已有图片</h3>

            <div class="image-grid">
              <article
                v-for="(image, index) in existingImages"
                :key="image.imageId ?? image.imgFileId"
                class="image-card"
              >
                <el-image
                  :src="resolveFileUrl(image.imgFileId)"
                  alt="已有商品图片"
                  fit="cover"
                  class="image-preview"
                >
                  <template #error>
                    <div class="image-placeholder">
                      图片加载失败
                    </div>
                  </template>
                </el-image>

                <span
                  v-if="index === 0"
                  class="cover-label"
                >
                  封面
                </span>

                <div class="image-actions">
                  <el-button
                    link
                    :disabled="
                      !canManageExistingImages ||
                      index === 0
                    "
                    @click="
                      moveExistingImage(index, index - 1)
                    "
                  >
                    上移
                  </el-button>

                  <el-button
                    link
                    :disabled="
                      !canManageExistingImages ||
                      index === existingImages.length - 1
                    "
                    @click="
                      moveExistingImage(index, index + 1)
                    "
                  >
                    下移
                  </el-button>

                  <el-button
                    link
                    type="danger"
                    :disabled="!canManageExistingImages"
                    @click="removeExistingImage(index)"
                  >
                    删除
                  </el-button>
                </div>
              </article>
            </div>
          </section>

          <section v-if="newImagePreviews.length > 0">
            <h3>本次新增图片</h3>

            <p
              v-if="existingImages.length > 0"
              class="section-tip"
            >
              新图片将在已有图片之后上传；当前契约无法在上传前为新图片取得 imageId。
            </p>

            <div class="image-grid">
              <article
                v-for="(preview, index) in newImagePreviews"
                :key="preview.id"
                class="image-card"
              >
                <img
                  :src="preview.url"
                  :alt="preview.file.name"
                  class="image-preview"
                  @error="
                    handleNewImagePreviewError(
                      preview.id
                    )
                  "
                >

                <span
                  v-if="
                    existingImages.length === 0 &&
                    index === 0
                  "
                  class="cover-label"
                >
                  封面
                </span>

                <p
                  class="file-name"
                  :title="preview.file.name"
                >
                  {{ preview.file.name }}
                </p>

                <div class="image-actions">
                  <el-button
                    link
                    :disabled="index === 0"
                    @click="
                      moveNewImage(index, index - 1)
                    "
                  >
                    上移
                  </el-button>

                  <el-button
                    link
                    :disabled="
                      index === newImagePreviews.length - 1
                    "
                    @click="
                      moveNewImage(index, index + 1)
                    "
                  >
                    下移
                  </el-button>

                  <el-button
                    link
                    type="danger"
                    @click="removeNewImage(index)"
                  >
                    删除
                  </el-button>
                </div>
              </article>
            </div>
          </section>

          <el-empty
            v-if="totalImageCount === 0"
            description="当前商品没有图片"
            :image-size="90"
          />
        </el-card>

        <div class="form-actions">
          <el-button
            :disabled="saving"
            @click="goBack"
          >
            取消
          </el-button>

          <el-button
            type="primary"
            :loading="saving"
            :disabled="saving"
            @click="saveProduct"
          >
            保存修改
          </el-button>
        </div>
      </el-form>
    </section>
  </main>
</template>

<style scoped>
.product-edit-page {
  min-height: calc(100vh - 72px);
  padding: 32px 20px 56px;
  background: #f5f7f6;
  color: #1e2a26;
}

.product-edit-container {
  width: min(1080px, 100%);
  margin: 0 auto;
}

.page-header,
.image-section-header,
.form-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
}

.page-header {
  margin-bottom: 22px;
  text-align: left;
}

.page-header h1 {
  margin: 0;
  color: #1e2a26;
  font-size: 30px;
}

.page-header p,
.image-section-header p {
  margin: 8px 0 0;
  color: #6c7a74;
}

.page-state {
  padding: 34px;
  background: #fff;
  border-radius: 14px;
}

.edit-form {
  text-align: left;
}

.form-card {
  margin-bottom: 18px;
  border: 1px solid #e3e9e6;
  border-radius: 14px;
}

.form-card h2 {
  margin: 0;
  color: #1e2a26;
  font-size: 20px;
}

.form-card h3 {
  margin: 22px 0 12px;
  color: #33413b;
  font-size: 16px;
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0 20px;
}

.full-control {
  width: 100%;
}

.field-error {
  width: 100%;
  margin-top: 7px;
  color: #d9544d;
  font-size: 13px;
}

.file-input {
  display: none;
}

.image-alert {
  margin-top: 16px;
}

.image-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 14px;
}

.image-card {
  position: relative;
  min-width: 0;
  padding: 10px;
  border: 1px solid #e3e9e6;
  border-radius: 12px;
}

.image-preview {
  display: block;
  width: 100%;
  aspect-ratio: 4 / 3;
  overflow: hidden;
  object-fit: cover;
  border-radius: 9px;
  background: #eef2f0;
}

.image-placeholder {
  display: flex;
  width: 100%;
  height: 100%;
  align-items: center;
  justify-content: center;
  color: #6c7a74;
}

.cover-label {
  position: absolute;
  top: 18px;
  left: 18px;
  padding: 4px 8px;
  color: #fff;
  background: #24735b;
  border-radius: 999px;
  font-size: 12px;
}

.file-name {
  overflow: hidden;
  margin-top: 8px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.image-actions {
  display: flex;
  flex-wrap: wrap;
  margin-top: 8px;
}

.image-actions :deep(.el-button + .el-button) {
  margin-left: 6px;
}

.section-tip {
  margin: -4px 0 12px;
  color: #8a6a35;
  font-size: 13px;
}

.form-actions {
  justify-content: flex-end;
}

</style>
