import type { RequestClientOptions } from '@vben/request';

import { useAppConfig } from '@vben/hooks';

import encryptFront from '#/api/encrypt';
import { baseRequestClient, requestClient } from '#/api/request';

const { sessionKey, ticketKey } = useAppConfig(
  import.meta.env,
  import.meta.env.PROD,
);

export namespace AuthApi {
  /** 登录接口参数 */
  export interface LoginParams {
    password?: string;
    username?: string;
  }

  /** 登录接口返回值 */
  export interface LoginResult {
    CorpId?: string;
    CorpName?: string;
    IsAdmin?: boolean;
    IsOurCompany?: boolean;
    RealName?: string;
    Ticket?: string;
    Token?: string;
    UserName?: string;
  }

  /** 修改密码参数 */
  export interface ChangePasswordParams {
    oldPassword: string;
    newPassword: string;
  }

  /** 公司信息 */
  export interface CorpInfo {
    CorpID: string;
    CName: string;
  }

  /** 刷新token返回值 */
  export interface RefreshTokenResult {
    data: string;
    status: number;
  }
}

const IMEI_KEY = 'GksybIMEI';
/**
 * 登录
 */
export async function loginApi(data: AuthApi.LoginParams) {
  const imei = await getIMEIApi();
  const request = {
    username: encryptFront(data.username || ''),
    password: encryptFront(data.password || ''),
    jsToken: { url: 'auth/loginToken' },
    imei,
  };
  const response = await requestClient.post<AuthApi.LoginResult>(
    'auth/login',
    request,
  );
  setTicketApi(response.Ticket || '');
  setTokenApi(response);
  return {
    accessToken: response.Token,
    token: response.Ticket,
  };
}

/**
 * 修改密码
 */
export async function changePasswordApi(data: AuthApi.ChangePasswordParams) {
  const request = {
    oldPassword: encryptFront(data.oldPassword || ''),
    newPassword: encryptFront(data.newPassword || ''),
    jsToken: 'auth/login',
  };
  return requestClient.post<string>('auth/changepassword', request);
}

/**
 * 获取用户所属公司
 */
export async function getUserCorpsApi() {
  return requestClient.post<AuthApi.CorpInfo[]>('auth/usercorps');
}

/**
 * 切换公司
 */
export async function changeCorpApi(data: string) {
  const request = {
    corpid: data,
  };
  const response = await requestClient.post<AuthApi.LoginResult>(
    'auth/changeCorp',
    request,
  );
  setTokenApi(response);
}

export async function getIMEIApi() {
  let imei = window.localStorage.getItem(IMEI_KEY);
  if (!imei) {
    imei = await requestClient.post<string>('auth/imei');
    window.localStorage.setItem(IMEI_KEY, imei);
  }
  return imei;
}

export function getTokenApi() {
  const json = window.localStorage.getItem(sessionKey);
  return json ? JSON.parse(json) : {};
}

export function setTokenApi(token: AuthApi.LoginResult | undefined) {
  if (!token) {
    window.localStorage.removeItem(sessionKey);
    return;
  }
  delete token.Ticket;
  window.localStorage.setItem(sessionKey, JSON.stringify(token));
}

export function getTicketApi() {
  return window.localStorage.getItem(ticketKey);
}

export function setTicketApi(ticket: string | undefined) {
  if (!ticket) {
    window.localStorage.removeItem(ticketKey);
    return;
  }
  window.localStorage.setItem(ticketKey, ticket);
}

/**
 * 加入jsToken
 */
export async function generateJsToken(config: RequestClientOptions) {
  if (!config.data) {
    return;
  }
  let token = config.data.jsToken;
  if (!token) {
    return;
  }
  delete config.data.jsToken;
  let url = 'auth/jsToken';
  if (token === true) {
    token = (config.url || '')
      .replaceAll(/^\/|(\?.*)$/g, '')
      .replace(/\/$/, '');
  }
  if (typeof token === 'string') {
    token = { key: token };
  } else {
    url = token?.url;
    token = token?.data;
  }
  const body = await requestClient.post<string>(url, token, {
    responseType: 'text',
    responseReturn: 'body',
  });
  const jqXHR = {
    setRequestHeader(name: string, value: string) {
      const headers: any = config.headers;
      headers[name] = value;
    },
  };
  eval(`; (function () {var jqXHR2 = ${JSON.stringify(jqXHR)}; ${body};})();`);
}

/**
 * 刷新accessToken
 */
export async function refreshTokenApi() {
  let ticket = getTicketApi();
  const response = await baseRequestClient.post<AuthApi.LoginResult>(
    'auth/refreshToken',
    undefined,
    {
      headers: { ticket },
      responseReturn: 'data',
    },
  );
  ticket = response.Ticket || '';
  setTicketApi(ticket);
  setTokenApi(response);
  return ticket;
}

/**
 * 退出登录
 */
export async function logoutApi() {
  const ticket = getTicketApi();
  await baseRequestClient.post('auth/exit', undefined, {
    headers: { ticket },
    responseReturn: 'data',
  });
  setTokenApi(undefined);
  setTicketApi(undefined);
}

/**
 * 获取用户权限码
 */
export async function getAccessCodesApi() {
  return [];
}
