import { defineUserConfig } from '@vuepress/cli'
import { defaultTheme } from '@vuepress/theme-default'
import { head, navbarZh, sidebarZh } from './configs'

export default defineUserConfig({
  base:"/docs/",
  lang: 'zh-CN',
  title: '港口事业部',
  description: '港口事业部开发框架',
  public:"./configs/public",
  head,
  theme: defaultTheme({
    logo: '/images/logo.png',
    docsDir: './framework/guide',
    locales: {
      '/': {
        navbar: navbarZh,
        selectLanguageName: '简体中文',
        selectLanguageText: '选择语言',
        selectLanguageAriaLabel: '选择语言',
        sidebar: sidebarZh,
        editLinkText: '在 GitHub 上编辑此页',
        lastUpdatedText: '上次更新',
        contributorsText: '贡献者',
        tip: '提示',
        warning: '注意',
        danger: '警告',
        notFound: [
          '这里什么都没有',
          '我们怎么到这来了？',
          '这是一个 404 页面',
          '看起来我们进入了错误的链接',
        ],
        backToHome: '返回首页',
        openInNewWindow: '在新窗口打开',
        toggleColorMode: '切换颜色模式',
        toggleSidebar: '切换侧边栏',
      },
    },
    themePlugins: {
    },
  })
})