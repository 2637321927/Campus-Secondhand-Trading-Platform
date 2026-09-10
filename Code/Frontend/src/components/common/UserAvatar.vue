<script setup lang="ts">
import { computed, watch } from 'vue'
import { useAvatarImage } from '../../composables/useAvatarImage'

const props = withDefaults(
  defineProps<{
    name?: string | null
    fileId?: number | null
    size?: number
    className?: string
  }>(),
  { size: 40 }
)

const { avatarUrl, loadAvatar } = useAvatarImage()

watch(
  () => props.fileId,
  (fileId) => {
    void loadAvatar(fileId).catch(() => undefined)
  },
  { immediate: true }
)

const fallbackText = computed(
  () => (props.name ?? '').slice(0, 1) || '用'
)
</script>

<template>
  <el-avatar
    :size="size"
    :src="avatarUrl || undefined"
    :class="className"
  >
    {{ fallbackText }}
  </el-avatar>
</template>
