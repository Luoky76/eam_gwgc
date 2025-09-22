import type { RouteRecordRaw } from 'vue-router';

import { useAppConfig } from '@vben/hooks';

import { IFrameView } from '#/layouts';

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);
const routes: RouteRecordRaw[] = [
  {
    meta: {
      title: '首页',
      hideInMenu: true,
      iframeSrc: `${location.origin}${apiURL}welcome.html`,
      affixTab: true,
    },
    name: 'Home',
    path: '/home',
    component: IFrameView,
  },
  {
    meta: {
      title: ' ',
      hideInBreadcrumb: true,
      hideInMenu: true,
      keepAlive: true,
    },
    name: 'IFrame',
    path: '/iframe',
    component: IFrameView,
  },
];

export default routes;
