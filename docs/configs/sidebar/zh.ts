import type { SidebarConfig } from '@vuepress/theme-default'

export const sidebarZh: SidebarConfig = {
  '/guide/': [
    {
      text: '指南',
      collapsible:false,
      children: [
        '/guide/readme.md',
        '/guide/sample.md',
        '/guide/upgrade.md',
        {
          text: '工具篇',
          collapsible:true,
          children: [
            '/guide/tool/visual-studio.md',
            '/guide/tool/apipost.md',
            '/guide/tool/fiddler.md',
          ],
        },
        {
          text: 'Web应用开发',
          collapsible:true,
          children: [
            '/guide/web/dependency-injection.md',
          ],
        },
      ],
    },
  ],
}
