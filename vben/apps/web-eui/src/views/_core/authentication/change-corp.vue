<script lang="ts" setup>
import type { AuthApi } from '#/api';

import { ref } from 'vue';

import {
  useVbenModal,
  VbenLabel,
  VbenRadioGroup,
  VbenRadioGroupItem,
  VbenSelect,
} from '@vben/common-ui';

import { changeCorpApi, getUserCorpsApi, getUserInfoApi } from '#/api';

defineOptions({ name: 'ChangeCorp' });

const corpId = ref<string>('');
const corps = ref<AuthApi.CorpInfo[]>([]);

const [Modal, modalApi] = useVbenModal({
  fullscreenButton: false,
  onCancel() {
    modalApi.close();
  },
  onConfirm: async () => {
    modalApi.lock();

    return changeCorpApi(corpId.value)
      .then(() => {
        modalApi.close();
      })
      .catch(() => {
        modalApi.unlock();
      });
  },
  onOpenChange(isOpen: boolean) {
    if (isOpen) {
      corpId.value = getUserInfoApi().corpId;
      getUserCorpsApi()
        .then((data) => {
          corps.value = data;
        })
        .catch(() => {});
    }
  },
  title: '切换组织',
});
</script>
<template>
  <Modal class="w-[400px]">
    <div class="mx-auto flex max-w-md flex-col items-center p-4">
      <template v-if="corps.length < 20">
        <VbenRadioGroup v-model="corpId" class="w-full">
          <template v-for="corp in corps" :key="corp.CorpID">
            <div class="flex items-center space-x-2 py-1">
              <VbenRadioGroupItem :id="corp.CorpID" :value="corp.CorpID" />
              <VbenLabel :for="corp.CorpID"> {{ corp.CName }}</VbenLabel>
            </div>
          </template>
        </VbenRadioGroup>
      </template>

      <!-- 当公司数量大于等于20个时使用Select -->
      <template v-else>
        <VbenSelect
          v-model="corpId"
          class="w-full"
          :options="
            corps.map((corp) => ({ label: corp.CName, value: corp.CorpID }))
          "
          placeholder="请选择组织"
        />
      </template>
    </div>
  </Modal>
</template>
