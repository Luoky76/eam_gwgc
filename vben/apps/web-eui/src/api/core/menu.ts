import { useAppConfig } from '@vben/hooks';

import { requestClient } from '#/api/request';

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);
const TARGET_TYPE = /[?&]targetType=([^&]+)/i;

/**
 * 获取用户所有菜单
 */
export async function getAllMenusApi() {
  const list = await requestClient.post<any[]>('auth/mymenus');
  const treeList = buildMenuTree(list);
  return toRoutes(treeList);
}

function toRoutes(list: any[]) {
  const routes: any[] = [];
  treeMap(list, null, (item, parent) => {
    const route = toRoute(item, parent ? parent.path : '');
    if (!parent) {
      routes.push(route);
      return route;
    }
    parent.children = parent.children || [];
    parent.children.push(route);
    return route;
  });
  return routes;
}

function toRoute(item: any, path: string) {
  const routePath = `${path}/${item.MENUNO}`;
  const url = processMenuUrl(item);
  if (!url) {
    return {
      name: item.MENUNO,
      path: routePath,
      children: [],
      meta: {
        pageId: item.MENUNO,
        title: item.MENUNAME,
        icon: item.MENUICON,
      },
    };
  }
  const isLink = item.targetType === 'blank';
  const route = {
    name: item.MENUNO,
    path: routePath,
    component: 'IFrameView',
    type: 'embedded',
    meta: {
      pageId: item.MENUNO,
      title: item.MENUNAME,
      icon: item.MENUICON,
      iframeSrc: isLink ? undefined : url,
      link: isLink ? url : undefined,
      keepAlive: true,
    },
  };
  return route;
}

/**
 * 构建菜单树形结构
 */
function buildMenuTree(
  data: any[],
  id = 'MENUNO',
  pid = 'MENUPARENTNO',
  childKey = 'children',
): any[] {
  // 按MENUPARENTNO分组，便于查找子节点
  const records: Record<string, any> = {};
  const result: any[] = [];

  // 遍历菜单列表，构建映射表
  for (const item of data) {
    const key = getKey(item[id]);
    if (key === null || key === undefined) continue;
    records[key] = item;
  }
  for (const item of data) {
    const key = getKey(item[pid]);
    const parentItem = records[key];
    if (!parentItem) {
      result.push(item);
      continue;
    }
    parentItem[childKey] = parentItem[childKey] || [];
    parentItem[childKey].push(item);
  }
  return result;
}

/**
 * 获取键值，处理0的情况
 */
function getKey(key: any): string {
  if (key === 0) return '0';
  return (key || '').toString();
}

function treeMap(
  list: any[],
  parent: any,
  f: (item: any, parent: any) => any,
): void {
  for (const column of list) {
    const route = f(column, parent);
    if (column.children?.length) {
      treeMap(column.children, route, f);
    }
  }
}

/**
 * 处理菜单URL
 */
function processMenuUrl(item: any): string {
  let url = item.MENUURL || '';
  if (url.length < 2) {
    return '';
  }
  url = url.replace(/.aspx/i, '.html');
  const result = url.match(TARGET_TYPE);
  if (result !== null && result.length > 0) {
    item.targetType = result[1];
  }
  url = url.replace(TARGET_TYPE, '');
  if (url.includes('javascript:') || url.indexOf('http') === 0) {
    item.targetType = item.targetType || 'blank';
    return url;
  }
  if (item.MENUNO) {
    url += url.includes('?') ? '&' : '?';

    url +=
      url.includes('?MenuNo=') || url.includes('&MenuNo=')
        ? `MenuNo2=${item.MENUNO}`
        : `MenuNo=${item.MENUNO}`;
  }

  return `${location.origin}${apiURL}${url}`;
}
