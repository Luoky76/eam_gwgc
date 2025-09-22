<script lang="ts" setup>
import { alert, useVbenForm, useVbenModal, z } from '@vben/common-ui';

import { changePasswordApi } from '#/api';

defineOptions({ name: 'ChangePassword' });

const [Form, formApi] = useVbenForm({
  handleSubmit: onSubmit,
  schema: [
    {
      component: 'VbenInputPassword',
      componentProps: {
        passwordStrength: true,
        placeholder: '请输入旧密码',
      },
      fieldName: 'oldPassword',
      label: '旧密码',
      rules: z.string().min(1, { message: '请输入旧密码' }),
    },
    {
      component: 'VbenInputPassword',
      componentProps: {
        passwordStrength: true,
        placeholder: '请输入新密码',
      },
      fieldName: 'newPassword',
      label: '新密码',
      rules: z.string().min(1, { message: '请输入新密码' }),
    },
    {
      component: 'VbenInputPassword',
      componentProps: {
        passwordStrength: true,
        placeholder: '再次输入新密码',
      },
      fieldName: 'equalPassword',
      label: '确认密码',
      rules: z.string().min(1, { message: '请输入确认密码' }),
    },
  ],
  showDefaultActions: false,
});

const [Modal, modalApi] = useVbenModal({
  fullscreenButton: false,
  onCancel() {
    modalApi.close();
  },
  onConfirm: async () => {
    await formApi.validateAndSubmitForm();
  },
  onOpenChange(isOpen: boolean) {
    if (isOpen) {
      const { values } = modalApi.getData<Record<string, any>>();
      if (values) {
        formApi.setValues(values);
      }
    }
  },
  title: '修改密码',
});

async function onSubmit(values: Record<string, any>) {
  modalApi.lock();
  return changePasswordApi({
    oldPassword: values.oldPassword,
    newPassword: values.newPassword,
  })
    .then(() => {
      modalApi.close();
      alert('密码修改成功');
    })
    .catch(() => {
      modalApi.unlock();
    });
}
</script>
<template>
  <Modal>
    <Form />
  </Modal>
</template>
