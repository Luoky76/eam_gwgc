<script lang="ts" setup>
import type { NotificationItem } from '@vben/layouts';

import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useVbenModal } from '@vben/common-ui';
import { useAppConfig, useWatermark } from '@vben/hooks';
import { addAPIProvider, MdiFamilyTree, MdiPasswordReset } from '@vben/icons';
import {
  BasicLayout,
  LockScreen,
  Notification,
  UserDropdown,
} from '@vben/layouts';
import { preferences } from '@vben/preferences';
import { getTabKey, useTabbarStore } from '@vben/stores';

import {
  fetchUnReadCount,
  fetchUnReadList,
  readAllMessage,
  readMessage,
} from '#/api/core/message';
import { useAuthStore } from '#/store';

import _changeCorpModal from '../views/_core/authentication/change-corp.vue';
import _changePasswordModal from '../views/_core/authentication/change-password.vue';

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD);

addAPIProvider('', {
  resources: [`${apiURL}iconify`],
});

const notifications = ref<NotificationItem[]>([]);
const unreadCount = ref<number>(0);
const authStore = useAuthStore();
const userInfo = authStore.fetchUserInfo();
const { destroyWatermark, updateWatermark } = useWatermark();
const showDot = computed(() => unreadCount.value > 0);
const [ChangePasswordModal, changePasswordModalApi] = useVbenModal({
  connectedComponent: _changePasswordModal,
});
const [ChangeCorpModal, changeCorpModalApi] = useVbenModal({
  connectedComponent: _changeCorpModal,
});

const router = useRouter();
const route = useRoute();
const tabbarStore = useTabbarStore();
/**
 * 导航到iframe页面
 * @param title 页面标题
 * @param url iframe地址
 */
function navigateToIframe(title: string, url: string) {
  const encodedUrl = encodeURIComponent(url);
  const encodedTitle = encodeURIComponent(title);
  const iframePath = `/iframe?url=${encodedUrl}&title=${encodedTitle}`;
  router.push(iframePath);
}

const menus = computed(() => [
  {
    handler: () => {
      changeCorpModalApi.open();
    },
    icon: MdiFamilyTree,
    text: '切换组织',
  },
  {
    handler: () => {
      changePasswordModalApi.open();
    },
    icon: MdiPasswordReset,
    text: '修改密码',
  },
]);

const avatar = computed(() => {
  return preferences.app.defaultAvatar;
});

async function handleLogout() {
  await authStore.logout(false);
}

async function refreshNotification() {
  notifications.value = await fetchUnReadList();
  unreadCount.value = notifications.value.filter((item) => !item.isRead).length;
}

async function handleOpen(callback: () => void) {
  await refreshNotification();
  callback();
}

async function handleRead(item: NotificationItem) {
  if (item.url) {
    navigateToIframe(item.message, `${apiURL}${item.url}`);
    return;
  }
  await readMessage(item.id);
  item.isRead = true;
  unreadCount.value = notifications.value.filter((item) => !item.isRead).length;
}

async function handleViewAll() {
  navigateToIframe('消息中心', `${apiURL}message/message-center.html`);
}

async function handleMakeAll() {
  await readAllMessage();
  await refreshNotification();
}

function initWindow() {
  const win = window as any;
  win.f_addTab = function (_tabid: string, title: string, url: string) {
    if (win.gksybConfigs && win.gksybConfigs.getUrl) {
      url = win.gksybConfigs.getUrl(url, win.gksybConfigs.urlBase);
    }
    navigateToIframe(title, url);
  };
  win.closeCurrentTab = async function () {
    await tabbarStore.closeTab(route, router);
  };
  win.getActivePageId = function () {
    return getTabKey(route);
  };
  win.editTabTitle = async function (pageId: string, title: string) {
    const tab = tabbarStore.getTabByKey(pageId);
    if (!tab) {
      return;
    }
    tabbarStore.setUpdateTime();
    await tabbarStore.setTabTitle(tab, title);
  };
  win._refreshNotification = refreshNotification;
}
initWindow();

onMounted(async () => {
  unreadCount.value = await fetchUnReadCount();
});

watch(
  () => preferences.app.watermark,
  async (enable) => {
    if (enable) {
      await updateWatermark({
        content: `${userInfo?.username} - ${userInfo?.realName}`,
      });
    } else {
      destroyWatermark();
    }
  },
  {
    immediate: true,
  },
);
</script>

<template>
  <ChangePasswordModal />
  <ChangeCorpModal />
  <BasicLayout @clear-preferences-and-logout="handleLogout">
    <template #user-dropdown>
      <UserDropdown
        :avatar
        :menus
        :text="userInfo?.realName"
        :description="userInfo?.desc"
        @logout="handleLogout"
      />
    </template>
    <template #notification>
      <Notification
        :dot="showDot"
        :notifications="notifications"
        @open="handleOpen"
        @read="handleRead"
        @make-all="handleMakeAll"
        @view-all="handleViewAll"
      />
    </template>
    <template #lock-screen>
      <LockScreen :avatar @to-login="handleLogout" />
    </template>
  </BasicLayout>
</template>
