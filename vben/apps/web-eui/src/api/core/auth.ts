import type { RequestClientOptions } from '@vben/request';
import { baseRequestClient, requestClient } from '#/api/request';
import encryptFront from '#/api/encrypt';

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

  export interface RefreshTokenResult {
    data: string;
    status: number;
  }
}

const IMEI_KEY = 'GksybIMEI';
const TOKEN_KEY = 'GksybData';
const TICKET_KEY = 'GksybTicket';
/**
 * 登录
 */
export async function loginApi(data: AuthApi.LoginParams) {
  let imei = await getIMEIApi();
  let request = {
    username: encryptFront(data.username || ''),
    password: encryptFront(data.password || ''),
    jsToken: { url: 'auth/loginToken' },
    imei: imei,
  };
  let response = await requestClient.post<AuthApi.LoginResult>(
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

export async function getIMEIApi() {
  let imei = window.localStorage.getItem(IMEI_KEY);
  if (!imei) {
    imei = await requestClient.post<string>('auth/imei');
    window.localStorage.setItem(IMEI_KEY, imei);
  }
  return imei;
}

export function getTokenApi() {
  let json = window.localStorage.getItem(TOKEN_KEY);
  return json ? JSON.parse(json) : {};
}

export function setTokenApi(token: AuthApi.LoginResult | undefined) {
  if (!token) {
    window.localStorage.removeItem(TOKEN_KEY);
    return;
  }
  delete token.Ticket;
  window.localStorage.setItem(TOKEN_KEY, JSON.stringify(token));
}

export function getTicketApi() {
  return window.localStorage.getItem(TICKET_KEY);
}

export function setTicketApi(ticket: string | undefined) {
  if (!ticket) {
    window.localStorage.removeItem(TICKET_KEY);
    return;
  }
  window.localStorage.setItem(TICKET_KEY, ticket);
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
    token = (config.url || '').replace(/^\/|(\?.*)$/g, '').replace(/\/$/, '');
  }
  if (typeof token === 'string') {
    token = { key: token };
  } else {
    url = token?.url;
    token = token?.data;
  }
  let body = await requestClient.post<string>(url, token, {
    responseType: 'text',
    responseReturn: 'body',
  });
  let jqXHR = {
    setRequestHeader: function (name: string, value: string) {
      let headers: any = config.headers;
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
  let response = await baseRequestClient.post<AuthApi.LoginResult>(
    'auth/refreshToken',
    undefined,
    {
      headers: { ticket: ticket },
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
  let ticket = getTicketApi();
  await baseRequestClient.post('auth/exit', undefined, {
    headers: { ticket: ticket },
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
