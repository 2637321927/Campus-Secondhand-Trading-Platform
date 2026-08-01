<script setup lang="ts">
import {
  onBeforeUnmount,
  onMounted,
  reactive,
  ref,
  nextTick,
  computed,
  watch
} from 'vue'
import { 
    ElMessage,
    ElMessageBox 
} from 'element-plus'
import type {
  FormInstance,
  FormRules
} from 'element-plus'
import { getCategories } from '../../api/modules/category'
import type {
  CategoryDto
} from '../../types/api/category'
import {
    isNavigationFailure,
    onBeforeRouteLeave,
    useRouter
} from 'vue-router'
import { createProduct } from '../../api/modules/product'
import type {
  CreateProductRequest
} from '../../types/api/product'

interface ProductPublishForm {
  name: string
  price: number | null
  categoryId: number | null
  info: string
  images: File[]
  shippingMethodId: number | null
  addressId: number | null
}

interface ProductPublishDraft {
  name: string
  price: number | null
  categoryId: number | null
  info: string
  shippingMethodId: number | null
  addressId: number | null
  activeStep: number
  updatedAt: string
}

interface ProductImagePreview {
  id: string
  file: File
  url: string
}

interface ShippingMethodOption {
  value: number
  label: string
  description: string
}

const shippingMethodOptions: ShippingMethodOption[] = [
  {
    value: 1,
    label: '当面交易',
    description: '与买家协商校内见面地点和时间'
  },
  {
    value: 2,
    label: '快递交易',
    description: '商品通过快递或校内配送方式交付'
  },
  {
    value: 3,
    label: '两者均可',
    description: '支持当面交易，也可以协商快递'
  }
]

const coverPreviewUrl = computed<string>(() => {
  if (imagePreviews.value.length === 0) {
    return ''
  }

  const firstImage = imagePreviews.value[0]

  if (firstImage === undefined) {
    return ''
  }

  return firstImage.url
})

const selectedCategoryName = computed<string>(() => {
  if (form.categoryId === null) {
    return '未选择'
  }

  for (const category of categories.value) {
    if (category.categoryId === form.categoryId) {
      return category.categoryName
    }
  }

  return '未选择'
})

const selectedShippingMethodLabel = computed<string>(() => {
  if (form.shippingMethodId === null) {
    return '未选择'
  }

  for (const method of shippingMethodOptions) {
    if (method.value === form.shippingMethodId) {
      return method.label
    }
  }

  return '未选择'
})

const formattedPrice = computed<string>(() => {
  if (form.price === null) {
    return '¥0.00'
  }

  const priceText = form.price.toFixed(2)

  return `¥${priceText}`
})

const activeStep = ref(0)

const router = useRouter()

const submitting = ref(false)
const publishedProductId = ref<number | null>(null)

const formRef = ref<FormInstance>()

const form = reactive<ProductPublishForm>({
  name: '',
  price: null,
  categoryId: null,
  info: '',
  images: [],
  shippingMethodId: null,
  addressId: null
})

const MAX_IMAGE_COUNT = 9
const MAX_IMAGE_SIZE = 5 * 1024 * 1024

const imageInputRef = ref<HTMLInputElement>()
const imagePreviews = ref<ProductImagePreview[]>([])
const failedPreviewIds = ref<string[]>([])

const categories = ref<CategoryDto[]>([])
const categoriesLoading = ref(false)
const categoriesError = ref('')

const rules: FormRules<ProductPublishForm> = {
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

  categoryId: [
    {
      required: true,
      message: '请选择商品分类',
      trigger: 'change'
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

  info: [
    {
     max: 100,
     message: '商品描述不能超过 100 个字符',
    trigger: 'blur'
    }
  ],

  images: [
   {
      validator: validateProductImages,
      trigger: 'change'
    }
  ],

  shippingMethodId: [
   {
     required: true,
     message: '请选择交易方式',
     trigger: 'change'
   }
 ] 
}

const DRAFT_STORAGE_KEY ='product_publish_draft'

let draftInitialized = false
let allowLeaveWithoutConfirm = false

const hasUnsavedChanges = ref(false)

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

function validateProductImages(
  _rule: unknown,
  value: File[],
  callback: (error?: Error) => void
): void {
  if (value.length === 0) {
    callback(new Error('请至少选择一张商品图片'))
    return
  }

  if (value.length > MAX_IMAGE_COUNT) {
    callback(
      new Error(`商品图片不能超过 ${MAX_IMAGE_COUNT} 张`)
    )
    return
  }

  if (failedPreviewIds.value.length > 0) {
    callback(
      new Error('存在无法预览的图片，请删除后重新选择')
    )
    return
  }

  callback()
}

function getInvalidStep(): number | null {
  if (
    !form.name.trim() ||
    form.name.trim().length > 30 ||
    form.price === null ||
    form.price <= 0 ||
    form.categoryId === null
  ) {
    return 0
  }

  if (
    form.images.length === 0 ||
    form.images.length > MAX_IMAGE_COUNT ||
    failedPreviewIds.value.length > 0 ||
    form.info.length > 100
  ) {
    return 1
  }

  if (form.shippingMethodId === null) {
    return 2
  }

  return null
}

function openImageSelector(): void {
  imageInputRef.value?.click()
}

function handleImageChange(event: Event): void {
  const input = event.target as HTMLInputElement
  const selectedFiles = Array.from(input.files ?? [])

  input.value = ''

  if (selectedFiles.length === 0) {
    return
  }

  const remainingCount =
    MAX_IMAGE_COUNT - form.images.length

  if (remainingCount <= 0) {
    ElMessage.warning(
      `最多只能选择 ${MAX_IMAGE_COUNT} 张图片`
    )
    return
  }

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

    const duplicated = form.images.some(
      (existingFile) =>
        existingFile.name === file.name &&
        existingFile.size === file.size &&
        existingFile.lastModified === file.lastModified
    )

    const duplicatedInCurrentSelection =
      validFiles.some((selectedFile) =>
        selectedFile.name === file.name &&
        selectedFile.size === file.size &&
        selectedFile.lastModified === file.lastModified
      )

    if (
      duplicated ||
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
      `最多只能选择 ${MAX_IMAGE_COUNT} 张图片`
    )
  }

  filesToAdd.forEach((file) => {
    form.images.push(file)

    imagePreviews.value.push({
      id: `${file.name}-${file.size}-${file.lastModified}`,
      file,
      url: URL.createObjectURL(file)
    })
  })

  formRef.value
    ?.validateField('images')
    .catch(() => undefined)
}

function removeImage(index: number): void {
  const preview = imagePreviews.value[index]

  if (preview) {
    URL.revokeObjectURL(preview.url)

    failedPreviewIds.value =
      failedPreviewIds.value.filter(
        (id) => id !== preview.id
      )
  }

  imagePreviews.value.splice(index, 1)
  form.images.splice(index, 1)

  formRef.value
    ?.validateField('images')
    .catch(() => undefined)
}

function handleImagePreviewError(
  previewId: string
): void {
  if (failedPreviewIds.value.includes(previewId)) {
    return
  }

  failedPreviewIds.value.push(previewId)

  ElMessage.error(
    '有图片无法正常预览，请删除后重新选择'
  )

  formRef.value
    ?.validateField('images')
    .catch(() => undefined)
}

function moveImage(
  currentIndex: number,
  targetIndex: number
): void {
  if (
    targetIndex < 0 ||
    targetIndex >= form.images.length
  ) {
    return
  }

  const [file] = form.images.splice(currentIndex, 1)
  const [preview] = imagePreviews.value.splice(
    currentIndex,
    1
  )

  if (!file || !preview) {
    return
  }

  form.images.splice(targetIndex, 0, file)
  imagePreviews.value.splice(
    targetIndex,
    0,
    preview
  )
}


function moveImagePrevious(index: number): void {
  moveImage(index, index - 1)
}

function moveImageNext(index: number): void {
  moveImage(index, index + 1)
}

function setCoverImage(index: number): void {
  if (index === 0) {
    return
  }

  moveImage(index, 0)
}

async function goNext(): Promise<void> {
  if (!formRef.value) {
    return
  }

  if (activeStep.value === 0) {
    try {
      await formRef.value.validateField([
        'name',
        'categoryId',
        'price'
      ])
    } catch {
      return
    }
  }

  if (activeStep.value === 1) {
    try {
      await formRef.value.validateField([
        'images',
        'info'
      ])
    } catch {
      return
    }
  }

  if (activeStep.value === 2) {
    try {
      await formRef.value.validateField(
        'shippingMethodId'
      )
    } catch {
      return
    }
  }

  if (activeStep.value < 3) {
    activeStep.value += 1
  }
}

function goPrevious(): void {
  if (activeStep.value > 0) {
    activeStep.value -= 1
  }
}

function hasFormContent(): boolean {
  if (form.name.trim() !== '') {
    return true
  }

  if (form.price !== null) {
    return true
  }

  if (form.categoryId !== null) {
    return true
  }

  if (form.info.trim() !== '') {
    return true
  }

  if (form.images.length > 0) {
    return true
  }

  if (form.shippingMethodId !== null) {
    return true
  }

  if (form.addressId !== null) {
    return true
  }

  return false
}

function createDraftData(): ProductPublishDraft {
  const draft: ProductPublishDraft = {
    name: form.name,
    price: form.price,
    categoryId: form.categoryId,
    info: form.info,
    shippingMethodId:
      form.shippingMethodId,
    addressId: form.addressId,
    activeStep: activeStep.value,
    updatedAt: new Date().toISOString()
  }

  return draft
}

function clearDraft(): void {
  try {
    localStorage.removeItem(
      DRAFT_STORAGE_KEY
    )
  } catch (error) {
    console.error('商品发布草稿清除失败', error)
  } finally {
    hasUnsavedChanges.value = false
  }
}

function saveDraft(): void {
  if (!draftInitialized) {
    return
  }

  if (!hasFormContent()) {
    clearDraft()
    return
  }

  const draft = createDraftData()
  const draftText = JSON.stringify(draft)

  hasUnsavedChanges.value = true

  try {
    localStorage.setItem(
      DRAFT_STORAGE_KEY,
      draftText
    )
  } catch (error) {
    console.error('商品发布草稿保存失败', error)
  }
}

function restoreDraft(): void {
  try {
    const savedDraft = localStorage.getItem(
      DRAFT_STORAGE_KEY
    )

    if (savedDraft === null) {
      return
    }

    const draft = JSON.parse(
      savedDraft
    ) as ProductPublishDraft

    if (typeof draft.name === 'string') {
      form.name = draft.name
    }

    if (
      draft.price === null ||
      typeof draft.price === 'number'
    ) {
      form.price = draft.price
    }

    if (
      draft.categoryId === null ||
      typeof draft.categoryId === 'number'
    ) {
      form.categoryId = draft.categoryId
    }

    if (typeof draft.info === 'string') {
      form.info = draft.info
    }

    if (
      draft.shippingMethodId === null ||
      typeof draft.shippingMethodId ===
        'number'
    ) {
      form.shippingMethodId =
        draft.shippingMethodId
    }

    if (
      draft.addressId === null ||
      typeof draft.addressId === 'number'
    ) {
      form.addressId = draft.addressId
    }

    if (
      typeof draft.activeStep === 'number'
    ) {
      let restoredStep = draft.activeStep

      if (restoredStep < 0) {
        restoredStep = 0
      }

      /*
       * 图片无法从 localStorage 恢复。
       * 因此最多恢复到第二步，
       * 让用户重新选择图片。
       */
      if (restoredStep > 1) {
        restoredStep = 1
      }

      activeStep.value = restoredStep
    }

    hasUnsavedChanges.value =
      hasFormContent()

    ElMessage.info(
      '已恢复上次填写的文字草稿，商品图片需要重新选择'
    )
  } catch (error) {
    console.error(
      '商品发布草稿读取失败',
      error
    )

    try {
      localStorage.removeItem(
        DRAFT_STORAGE_KEY
      )
    } catch (removeError) {
      console.error(
        '无效商品发布草稿清除失败',
        removeError
      )
    }
  } finally {
    draftInitialized = true
  }
}

function handleBeforeUnload(
  event: BeforeUnloadEvent
): void {
  if (allowLeaveWithoutConfirm) {
    return
  }

  if (!hasUnsavedChanges.value) {
    return
  }

  event.preventDefault()
  event.returnValue = ''
}

async function loadCategories(): Promise<void> {
  categoriesLoading.value = true
  categoriesError.value = ''

  try {
    const response = await getCategories()

    categories.value = response.data
  } catch (error) {
    categories.value = []
    categoriesError.value = '商品分类加载失败，请重新尝试'

    console.error(error)
  } finally {
    categoriesLoading.value = false
  }
}

async function showInvalidStep(
  step: number
): Promise<void> {
  activeStep.value = step

  await nextTick()

  if (!formRef.value) {
    return
  }

  const fields =
    step === 0
      ? ['name', 'categoryId', 'price']
      : step === 1
        ? ['images', 'info']
        : ['shippingMethodId']

  await formRef.value
    .validateField(fields)
    .catch(() => undefined)
}

async function submitProduct(): Promise<void> {
  if (
    submitting.value ||
    publishedProductId.value !== null
  ) {
    return
  }

  const invalidStep = getInvalidStep()

  if (invalidStep !== null) {
    ElMessage.warning(
      '请完善商品信息后再发布'
    )

    await showInvalidStep(invalidStep)
    return
  }

  if (
    form.price === null ||
    form.categoryId === null ||
    form.shippingMethodId === null
  ) {
    return
  }

  const requestData: CreateProductRequest = {
    name: form.name.trim(),
    price: form.price,
    categoryId: form.categoryId,
    images: [...form.images],
    shippingMethodId:
      form.shippingMethodId,
    saveAsDraft: false
  }

  const trimmedInfo = form.info.trim()

  if (trimmedInfo) {
    requestData.info = trimmedInfo
  }

  if (form.addressId !== null) {
    requestData.addressId = form.addressId
  }

  submitting.value = true

  let createdProductId: number

  try {
    const response = await createProduct(
      requestData
    )

    const productId =
      response.data.productId

    if (
      !Number.isInteger(productId) ||
      productId <= 0
    ) {
      throw new Error(
        '发布接口未返回有效的商品 ID'
      )
    }

    createdProductId = productId
    publishedProductId.value = productId
  } catch (error) {
    console.error(error)

    ElMessage.error(
      '商品发布失败，请检查网络或稍后重试'
    )
    return
  } finally {
    submitting.value = false
  }

  clearDraft()
  allowLeaveWithoutConfirm = true
  ElMessage.success('商品发布成功')

  try {
    const navigationFailure = await router.push({
      name: 'product-detail',
      params: {
        productId: createdProductId
      }
    })

    if (isNavigationFailure(navigationFailure)) {
      throw new Error('商品详情页导航被取消')
    }
  } catch (error) {
    console.error(
      '商品已发布，但页面跳转失败：',
      error
    )

    ElMessage.warning(
      '商品已经发布，但详情页跳转失败，请从我的商品中查看'
    )
  }
}

onMounted(() => {
  restoreDraft()
  loadCategories()

  window.addEventListener(
    'beforeunload',
    handleBeforeUnload
  )
})

onBeforeUnmount(() => {
  window.removeEventListener(
    'beforeunload',
    handleBeforeUnload
  )

  for (const preview of imagePreviews.value) {
    URL.revokeObjectURL(preview.url)
  }
})

watch(
  [
    () => form.name,
    () => form.price,
    () => form.categoryId,
    () => form.info,
    () => form.images.length,
    () => form.shippingMethodId,
    () => form.addressId,
    () => activeStep.value
  ],
  () => {
    saveDraft()
  }
)

onBeforeRouteLeave(async () => {
  if (allowLeaveWithoutConfirm) {
    return true
  }

  if (!hasUnsavedChanges.value) {
    return true
  }

  try {
    await ElMessageBox.confirm(
      '当前商品信息尚未发布，离开后文字草稿仍会保留，但图片需要重新选择。',
      '确认离开发布页',
      {
        confirmButtonText: '确认离开',
        cancelButtonText: '继续编辑',
        type: 'warning'
      }
    )

    return true
  } catch {
    return false
  }
})
</script>

<template>
  <main class="publish-page">
    <section class="publish-container">
      <header class="publish-header">
        <div>
          <h1>发布闲置</h1>

          <p>
            填写商品信息，让校友更快发现你的闲置物品。
          </p>
        </div>
      </header>

      <el-card
        class="publish-card"
        shadow="never"
      >
        <el-steps
          :active="activeStep"
          finish-status="success"
          align-center
        >
          <el-step title="基本信息" />
          <el-step title="图片与描述" />
          <el-step title="交易方式" />
          <el-step title="预览发布" />
        </el-steps>

        <el-form
          ref="formRef"
          :model="form"
          :rules="rules"
          label-position="top"
          class="publish-form"
        >
          <div class="step-content">
            <!-- 第一步：基本信息 -->
            <section
              v-if="activeStep === 0"
              class="form-step"
            >
              <div class="step-heading">
                <h2>基本信息</h2>

                <p>
                  填写商品的名称、分类和价格。
                </p>
              </div>

              <el-form-item
                label="商品名称"
                prop="name"
              >
                <el-input
                  v-model="form.name"
                  maxlength="30"
                  show-word-limit
                  clearable
                  placeholder="例如：数据库系统概论第七版"
                />
              </el-form-item>

              <el-form-item
                label="商品分类"
                prop="categoryId"
              >
                <el-select
                  v-model="form.categoryId"
                  :loading="categoriesLoading"
                  :disabled="categoriesLoading"
                  clearable
                  filterable
                  placeholder="请选择商品分类"
                  class="form-control"
                >
                  <el-option
                    v-for="category in categories"
                    :key="category.categoryId"
                    :label="
                      category.categoryName
                    "
                    :value="
                      category.categoryId
                    "
                  />
                </el-select>

                <div
                  v-if="categoriesError"
                  class="category-error"
                >
                  <span>
                    {{ categoriesError }}
                  </span>

                  <el-button
                    link
                    type="primary"
                    :loading="
                      categoriesLoading
                    "
                    @click="loadCategories"
                  >
                    重新加载
                  </el-button>
                </div>
              </el-form-item>

              <el-form-item
                label="商品价格"
                prop="price"
              >
                <div class="price-field">
                  <el-input-number
                    v-model="form.price"
                    :min="0.01"
                    :precision="2"
                    :step="1"
                    controls-position="right"
                    placeholder="请输入商品价格"
                    class="form-control"
                  />

                  <span class="price-unit">
                    元
                  </span>
                </div>
              </el-form-item>
            </section>

            <!-- 第二步：图片与描述 -->
            <section
              v-else-if="
                activeStep === 1
              "
              class="form-step"
            >
              <div class="step-heading">
                <h2>图片与描述</h2>

                <p>
                  上传清晰的实物图片，并说明商品的实际情况。
                </p>
              </div>

              <el-form-item
                label="商品图片"
                prop="images"
              >
                <input
                  ref="imageInputRef"
                  class="image-file-input"
                  type="file"
                  accept="image/*"
                  multiple
                  @change="
                    handleImageChange
                  "
                >

                <div
                  class="image-upload-toolbar"
                >
                  <el-button
                    type="primary"
                    plain
                    :disabled="
                      form.images.length >=
                      MAX_IMAGE_COUNT
                    "
                    @click="
                      openImageSelector
                    "
                  >
                    选择图片
                  </el-button>

                  <span
                    class="image-upload-tip"
                  >
                    已选择
                    {{ form.images.length }}
                    /
                    {{ MAX_IMAGE_COUNT }}
                    张，单张不超过 5 MB
                  </span>
                </div>

                <div
                  v-if="
                    imagePreviews.length > 0
                  "
                  class="image-preview-grid"
                >
                  <article
                    v-for="(
                      preview,
                      index
                    ) in imagePreviews"
                    :key="preview.id"
                    class="image-preview-card"
                  >
                    <div
                      class="image-preview-main"
                    >
                      <img
                        :src="preview.url"
                        :alt="
                          `商品图片 ${
                            index + 1
                          }`
                        "
                        @error="
                          handleImagePreviewError(
                            preview.id
                          )
                        "
                      >

                      <span
                        v-if="index === 0"
                        class="cover-label"
                      >
                        封面
                      </span>
                    </div>

                    <p
                      class="image-file-name"
                      :title="
                        preview.file.name
                      "
                    >
                      {{
                        preview.file.name
                      }}
                    </p>

                    <div
                      class="image-actions"
                    >
                      <el-button
                        link
                        type="primary"
                        :disabled="
                          index === 0
                        "
                        @click="
                          setCoverImage(
                            index
                          )
                        "
                      >
                        设为封面
                      </el-button>

                      <el-button
                        link
                        :disabled="
                          index === 0
                        "
                        @click="
                          moveImagePrevious(
                            index
                          )
                        "
                      >
                        上移
                      </el-button>

                      <el-button
                        link
                        :disabled="
                          index ===
                          imagePreviews.length -
                            1
                        "
                        @click="
                          moveImageNext(
                            index
                          )
                        "
                      >
                        下移
                      </el-button>

                      <el-button
                        link
                        type="danger"
                        @click="
                          removeImage(index)
                        "
                      >
                        删除
                      </el-button>
                    </div>
                  </article>
                </div>

                <button
                  v-if="
                    form.images.length <
                    MAX_IMAGE_COUNT
                  "
                  type="button"
                  class="image-add-card"
                  @click="
                    openImageSelector
                  "
                >
                  <span
                    class="image-add-symbol"
                  >
                    ＋
                  </span>

                  <span>
                    继续添加图片
                  </span>

                  <small>
                    首张图片将作为封面
                  </small>
                </button>
              </el-form-item>

              <el-form-item
                label="商品描述"
                prop="info"
              >
                <el-input
                  v-model="form.info"
                  type="textarea"
                  :rows="6"
                  maxlength="100"
                  show-word-limit
                  resize="vertical"
                  placeholder="请说明商品的品牌、型号、成色、使用情况以及是否存在瑕疵"
                />
              </el-form-item>
            </section>

            <!-- 第三步：交易方式 -->
            <section
              v-else-if="
                activeStep === 2
              "
              class="form-step"
            >
              <div class="step-heading">
                <h2>交易方式</h2>

                <p>
                  请选择你能够提供的商品交付方式。
                </p>
              </div>

              <el-form-item
                label="支持的交易方式"
                prop="shippingMethodId"
              >
                <el-radio-group
                  v-model="
                    form.shippingMethodId
                  "
                  class="shipping-method-grid"
                >
                  <el-radio
                    v-for="
                      option in
                      shippingMethodOptions
                    "
                    :key="option.value"
                    :value="option.value"
                    class="shipping-method-option"
                    border
                  >
                    <span
                      class="shipping-method-content"
                    >
                      <strong>
                        {{ option.label }}
                      </strong>

                      <small>
                        {{
                          option.description
                        }}
                      </small>
                    </span>
                  </el-radio>
                </el-radio-group>
              </el-form-item>

              <el-alert
                title="交易地址功能说明"
                type="info"
                :closable="false"
                show-icon
                class="address-notice"
              >
                <template #default>
                  <p
                    class="address-notice-text"
                  >
                    地址列表接口的响应字段尚未最终确定，
                    当前发布时可以暂不选择地址。
                    后续接入地址管理模块后，再通过
                    <code>addressId</code>
                    选择当前用户保存的交易地址。
                  </p>
                </template>
              </el-alert>

              <div class="transaction-tips">
                <h3>校园交易提醒</h3>

                <ul>
                  <li>
                    当面交易建议选择校内公共区域。
                  </li>

                  <li>
                    交易前请和买家确认商品状态及交易时间。
                  </li>

                  <li>
                    快递交易请保留寄件和物流凭证。
                  </li>
                </ul>
              </div>
            </section>

            <!-- 第四步：预览发布 -->
            <section
              v-else
              class="form-step preview-step"
            >
              <div class="step-heading">
                <h2>预览发布</h2>

                <p>
                  请确认以下商品信息，发布后仍可在“我的商品”中修改。
                </p>
              </div>

              <div
                class="publish-preview-layout"
              >
                <div
                  class="publish-preview-images"
                >
                  <div
                    class="publish-preview-cover"
                  >
                    <img
                      v-if="coverPreviewUrl"
                      :src="coverPreviewUrl"
                      alt="商品封面预览"
                    >

                    <div
                      v-else
                      class="publish-preview-empty"
                    >
                      暂无封面图片
                    </div>

                    <span
                      v-if="coverPreviewUrl"
                      class="preview-cover-label"
                    >
                      封面
                    </span>
                  </div>

                  <div
                    v-if="
                      imagePreviews.length >
                      1
                    "
                    class="publish-preview-thumbnails"
                  >
                    <img
                      v-for="(
                        preview,
                        index
                      ) in imagePreviews"
                      :key="preview.id"
                      :src="preview.url"
                      :alt="
                        `预览图片 ${
                          index + 1
                        }`
                      "
                      @error="
                        handleImagePreviewError(
                          preview.id
                        )
                      "
                    >
                  </div>
                </div>

                <div
                  class="publish-preview-summary"
                >
                  <div
                    class="preview-status-row"
                  >
                    <el-tag
                      type="warning"
                      effect="light"
                    >
                      待发布
                    </el-tag>

                    <span>
                      共
                      {{
                        imagePreviews.length
                      }}
                      张图片
                    </span>
                  </div>

                  <h3 class="preview-title">
                    {{ form.name.trim() }}
                  </h3>

                  <p class="preview-price">
                    {{ formattedPrice }}
                  </p>

                  <dl class="preview-meta">
                    <div>
                      <dt>商品分类</dt>

                      <dd>
                        {{
                          selectedCategoryName
                        }}
                      </dd>
                    </div>

                    <div>
                      <dt>交易方式</dt>

                      <dd>
                        {{
                          selectedShippingMethodLabel
                        }}
                      </dd>
                    </div>

                    <div>
                      <dt>交易地址</dt>

                      <dd>
                        {{
                          form.addressId ===
                          null
                            ? '暂未选择'
                            : `地址 ${form.addressId}`
                        }}
                      </dd>
                    </div>
                  </dl>
                </div>
              </div>

              <div
                class="preview-description"
              >
                <h3>商品描述</h3>

                <p v-if="form.info.trim()">
                  {{ form.info.trim() }}
                </p>

                <p
                  v-else
                  class="preview-description-empty"
                >
                  卖家暂未填写商品描述。
                </p>
              </div>

              <el-alert
                title="发布前请再次确认商品信息真实准确"
                type="warning"
                :closable="false"
                show-icon
                class="publish-confirm-alert"
              >
                <template #default>
                  商品发布后，其他用户将能够查看商品信息。
                  请勿上传包含手机号、身份证、宿舍门牌等隐私信息的图片或描述。
                </template>
              </el-alert>
            </section>
          </div>
        </el-form>

        <footer class="step-actions">
          <el-button
            :disabled="
              activeStep === 0 ||
              submitting ||
              publishedProductId !== null
            "
            @click="goPrevious"
          >
            上一步
          </el-button>

          <el-button
            v-if="activeStep < 3"
            type="primary"
            :disabled="
              submitting ||
              publishedProductId !== null
            "
            @click="goNext"
          >
            下一步
          </el-button>

          <el-button
            v-else
            type="primary"
            :loading="submitting"
            :disabled="
              submitting ||
              publishedProductId !== null
            "
            @click="submitProduct"
          >
            {{
              publishedProductId === null
                ? '发布商品'
                : '商品已发布'
            }}
          </el-button>
        </footer>
      </el-card>
    </section>
  </main>
</template>

<style scoped>
.publish-page {
  min-height: calc(100vh - 72px);
  padding: 32px 20px 56px;
  background: #f5f7f6;
}

.publish-container {
  width: min(100%, 1080px);
  margin: 0 auto;
}

.publish-header {
  margin-bottom: 24px;
}

.publish-header h1 {
  margin: 0;
  color: #1e2a26;
  font-size: 30px;
}

.publish-header p {
  margin: 8px 0 0;
  color: #6c7a74;
}

.publish-card {
  border: 1px solid #e3e9e6;
  border-radius: 18px;
}

.step-content {
  min-height: 320px;
  margin-top: 36px;
  padding: 28px;
  border-radius: 14px;
  background: #f8faf9;
}

.step-content h2 {
  margin-top: 0;
  color: #1e2a26;
}

.step-content p {
  color: #6c7a74;
}

.step-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 28px;
}

.publish-form {
  margin-top: 36px;
}

.form-step {
  max-width: 680px;
  margin: 0 auto;
}

.step-heading {
  margin-bottom: 28px;
}

.step-heading h2 {
  margin: 0;
  color: #1e2a26;
}

.step-heading p {
  margin: 8px 0 0;
  color: #6c7a74;
}

.form-control {
  width: 100%;
}

.category-error {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 8px;
  color: #d9544d;
  font-size: 13px;
}

.price-unit {
  margin-left: 10px;
  color: #6c7a74;
}

.image-file-input {
  display: none;
}

.image-upload-toolbar {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
  width: 100%;
  margin-bottom: 18px;
}

.image-upload-tip {
  color: #6c7a74;
  font-size: 13px;
}

.image-preview-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
  width: 100%;
}

.image-preview-card {
  min-width: 0;
  padding: 12px;
  border: 1px solid #e3e9e6;
  border-radius: 14px;
  background: #ffffff;
}

.image-preview-main {
  position: relative;
  overflow: hidden;
  aspect-ratio: 4 / 3;
  border-radius: 12px;
  background: #eef2f0;
}

.image-preview-main img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.cover-label {
  position: absolute;
  top: 10px;
  left: 10px;
  padding: 4px 9px;
  border-radius: 999px;
  color: #ffffff;
  background: #24735b;
  font-size: 12px;
}

.image-file-name {
  overflow: hidden;
  margin: 10px 0 4px;
  color: #1e2a26;
  font-size: 13px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.image-actions {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 2px;
}

.image-add-card {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  gap: 5px;
  width: 100%;
  min-height: 116px;
  margin-top: 16px;
  border: 1px dashed #9ab7ac;
  border-radius: 14px;
  color: #24735b;
  background: #f5faf8;
  cursor: pointer;
}

.image-add-card:hover {
  border-color: #24735b;
  background: #edf7f3;
}

.image-add-symbol {
  font-size: 28px;
  line-height: 1;
}

.image-add-card small {
  color: #6c7a74;
}

.shipping-method-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 14px;
  width: 100%;
}

.shipping-method-option {
  width: 100%;
  height: auto;
  min-height: 104px;
  margin: 0;
  padding: 18px;
  border-radius: 14px;
  background: #ffffff;
}

.shipping-method-option.is-checked {
  border-color: #24735b;
  background: #f0f8f5;
}

.shipping-method-option :deep(.el-radio__label) {
  width: 100%;
  padding-left: 12px;
  white-space: normal;
}

.shipping-method-content {
  display: flex;
  flex-direction: column;
  gap: 7px;
}

.shipping-method-content strong {
  color: #1e2a26;
  font-size: 15px;
}

.shipping-method-content small {
  color: #6c7a74;
  font-size: 13px;
  line-height: 1.6;
}

.address-notice {
  margin-top: 8px;
  border-radius: 12px;
}

.address-notice-text {
  margin: 0;
  line-height: 1.7;
}

.address-notice code {
  padding: 2px 5px;
  border-radius: 5px;
  color: #24735b;
  background: #e7f1ed;
}

.transaction-tips {
  margin-top: 22px;
  padding: 18px 20px;
  border: 1px solid #e3e9e6;
  border-radius: 14px;
  background: #f8faf9;
}

.transaction-tips h3 {
  margin: 0 0 12px;
  color: #1e2a26;
  font-size: 16px;
}

.transaction-tips ul {
  margin: 0;
  padding-left: 20px;
  color: #6c7a74;
  font-size: 14px;
  line-height: 1.9;
}

.preview-step {
  max-width: 860px;
}

.publish-preview-layout {
  display: grid;
  grid-template-columns:
    minmax(0, 1.1fr)
    minmax(280px, 0.9fr);
  gap: 28px;
  padding: 20px;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  background: #ffffff;
}

.publish-preview-cover {
  position: relative;
  overflow: hidden;
  aspect-ratio: 4 / 3;
  border-radius: 14px;
  background: #eef2f0;
}

.publish-preview-cover img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.publish-preview-empty {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 100%;
  height: 100%;
  color: #6c7a74;
}

.preview-cover-label {
  position: absolute;
  top: 12px;
  left: 12px;
  padding: 5px 10px;
  border-radius: 999px;
  color: #ffffff;
  background: #24735b;
  font-size: 12px;
}

.publish-preview-thumbnails {
  display: grid;
  grid-template-columns:
    repeat(5, minmax(0, 1fr));
  gap: 8px;
  margin-top: 10px;
}

.publish-preview-thumbnails img {
  width: 100%;
  aspect-ratio: 1;
  border: 1px solid #e3e9e6;
  border-radius: 9px;
  object-fit: cover;
}

.publish-preview-summary {
  min-width: 0;
}

.preview-status-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  color: #6c7a74;
  font-size: 13px;
}

.preview-title {
  margin: 22px 0 10px;
  color: #1e2a26;
  font-size: 25px;
  line-height: 1.45;
  word-break: break-word;
}

.preview-price {
  margin: 0 0 26px;
  color: #d9544d;
  font-size: 30px;
  font-weight: 700;
}

.preview-meta {
  display: grid;
  gap: 0;
  margin: 0;
}

.preview-meta div {
  display: flex;
  justify-content: space-between;
  gap: 20px;
  padding: 14px 0;
  border-bottom: 1px solid #e3e9e6;
}

.preview-meta dt {
  color: #6c7a74;
}

.preview-meta dd {
  margin: 0;
  color: #1e2a26;
  font-weight: 500;
  text-align: right;
}

.preview-description {
  margin-top: 20px;
  padding: 20px;
  border: 1px solid #e3e9e6;
  border-radius: 16px;
  background: #ffffff;
}

.preview-description h3 {
  margin: 0 0 12px;
  color: #1e2a26;
  font-size: 17px;
}

.preview-description p {
  margin: 0;
  color: #46534e;
  line-height: 1.8;
  white-space: pre-wrap;
  word-break: break-word;
}

.preview-description-empty {
  color: #8a9691 !important;
}

.publish-confirm-alert {
  margin-top: 20px;
  border-radius: 12px;
}

</style>
