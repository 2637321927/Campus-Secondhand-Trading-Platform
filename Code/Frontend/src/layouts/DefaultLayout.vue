<script setup lang="ts">
import AppHeader from '../components/layout/AppHeader.vue';
</script>

<template>
  <div class="default-layout">
    <AppHeader />

    <div class="default-content">
      <router-view />
    </div>
  </div>
</template>

<style scoped>
.default-layout {
  min-height: 100vh;
  background: #f5f7f6;
}

.default-content {
  min-height: calc(100vh - 72px);
}
</style>
