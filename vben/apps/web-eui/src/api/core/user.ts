import type { UserInfo } from '@vben/types';
import { getTokenApi } from './auth';


/**
 * 获取用户信息
 */
export function getUserInfoApi() {
  let info = getTokenApi();
  let userInfo: UserInfo = {
    avatar: '',
    realName: info.RealName,
    roles: [],
    userId: "-1",
    username: info.UserName,
    desc: info.CorpName,
    homePath: '',
    token: info.Token,
  };
  return userInfo;
}
