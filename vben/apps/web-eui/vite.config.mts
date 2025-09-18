import { defineConfig } from '@vben/vite-config';

export default defineConfig(async () => {
  return {
    application: {
      license:false,
    },
    vite: {
      server: {
        proxy: {
          '/api': {
            changeOrigin: true,
            rewrite: (path) => path.replace(/^\/api/, ''),
            // mock代理目标地址
            target: 'http://localhost:57969',
            ws: true,
          },
        },
      },
      // 更全面的构建配置，彻底禁用代码分割
      build: {
        rollupOptions: {
          output: {
            // 禁用动态导入的代码分割
            manualChunks: undefined,
            // 强制所有代码合并到一个文件
            inlineDynamicImports: true,
          }
        }
      },
    },
  };
});
