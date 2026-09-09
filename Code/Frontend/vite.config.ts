import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      // 将以 /api 开头的请求代理到后端
      '/api': {
        target: 'http://localhost:5141', // 你的后端地址
        changeOrigin: true,
        // 如果后端接口路径不带 /api 前缀，可以去掉
        // rewrite: (path) => path.replace(/^\/api/, '')
      }
    }
  }
})