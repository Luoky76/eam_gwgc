import type { UserInfo } from '@vben/types';

import { getTokenApi } from './auth';

/**
 * 获取用户信息
 */
export function getUserInfoApi() {
  const info = getTokenApi();
  const userInfo: UserInfo = {
    avatar: '',
    realName: info.RealName,
    roles: [],
    userId: '-1',
    username: info.UserName,
    desc: info.CorpName,
    homePath: '',
    token: info.Token,
    corpId: info.CorpId,
    corpName: info.CorpName,
  };
  return userInfo;
}
