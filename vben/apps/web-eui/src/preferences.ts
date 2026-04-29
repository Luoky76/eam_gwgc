import { defineOverridesPreferences } from '@vben/preferences';

/**
 * @description 项目配置文件
 * 只需要覆盖项目中的一部分配置，不需要的配置不用覆盖，会自动使用默认配置
 * !!! 更改配置后请清空缓存，否则可能不生效
 */
export const overridesPreferences = defineOverridesPreferences({
  // overrides
  app: {
    name: import.meta.env.VITE_APP_TITLE,
    enableRefreshToken: true,
    accessMode: 'mixed',
    defaultHomePath: '/home',
    watermark: true,
    defaultAvatar:
      'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAEfElEQVR4AexZXYgcRRCumr3158SfGEWc6ZlZZyY/oiDxwGASjIr6oigI4h958tB3xZ97UBBEgz/45IOeL4qiCKKSiHqIHv6hQiTxSeR2NZs5OS9GDOTBXHanUn1JYG9JuO6Z7rnbu2m6tmd6q76q/rq6Z3bbgVVeKgJWeQJAlQFVBqxyBqolsMoToNoEqyWwVEvAdddf4obJtSybPG/j2qWKo9QM8ILoDi+I32eZcYa6Bx2iPSy/YO3YP7LPC+P3guCKW8skoxQCLo+iwAuSCQTchQD3slzWP0jZhwT3ZeBMiCDeLcR6r1/Hxr11AtwguqXWwb0IpDOzt4PT3ScayY02Bt2LaZUAt9HY6AB+wg7XsOjWtZDRp2G47kpdQx19R0dZV9fJarvZZpglbx3uUvZRXuPT2fX3WSNABNFT7CxmKVo3iDB+vCjImeytEQCAT4KpQijJNIW2AMcKAfw4u5m9XMRiqNLFvr/uBkNgC2CsEIBE2xZ4MXCTgXlMGZYVAgDQ/DPcAQEWiiUCqMjOf9ph8hujcUzpyAoBiHBQgpsUIvOYMj4rBGSE+yS4UXFor1G8k2BWCOjU4bOT+MaazlFnwhhYD5AVAv5uNmcB6JseP0Uvv5qZmTK+rGRQVgiQwJDhC/OtgQ9+BBrD6g/HGgFp2vwcqPhS4F+Ru/5qt77sD9zUvTUCZIBn1el+JqEtr3NKsz4EO3LaKplZJaDVah0Gqm0BwJ9Bv/wAWW37PIa+rbKFVQJkFGn6+3TantrMm+IY3x9hWaweAYQn0nZzq7RdTLno99YJOBVg2m7t7MxhhAhPMxl/nOrvaZvcP0bdeiPd33ypp7/Q5WLGpREgA5GPsgP7m88xGRHPMPZJwv07p6d/OyR1y5JSCShrUDp+KgJ02FqJulUG2J5VecDhBvFWEUYP8uHImBdGr4ggHveD+AMRRF9IOXEdj3t+/LLUYd0H3DDe4vsbXNvxGc8Az0uECOKHhR9/yO1/fMCRspPvgPAdfq19Hgkf5UGNEsA9AHiblBPXMMqPyMekDuu+6xB8T9iZZox/5wkKk4dsEMKxQeHiuu6wCJNRESQ/YY0OMODr/DJzN7cXshSta+YJInpTEsJniN9yluxIkuTsosDSvjABIoiecYbOmQWicX6RuU6C2hQE2MZZ8vb/czQrDJwX5CYgCJKrhJ/wvzT4LACeB+WXC4DgRRFGPza45HWfiwDfT+7KgPYA0jV5HRuzI9zcyWq/CpFsz4OpTQCvwUcI6WN2ZmQNMo6Jej44NMl7A2+senBaBPh+fDWvwdf0XJSnzXvDW64b+zoetQgghDcYvMayXOu5zhC+qhOcMgG8z8izvut1wJdGl+4cGRmpq/pWJuAY1Tapgi6lHvuuzxw6PMKtUlUmAAgvVUJcDkoZKO8D6gQsh4FZiKEiwAKpAwVZZYDqdHXn4Gtw8KZBEI51UnVcyhkg/9FN/5yaHASRsRonQBVw0PSUM2DQBqYab0WAKlMrVa/KgJU6s6rjqjJAlamVqjfwGVB0Yo4DAAD//z3e6GgAAAAGSURBVAMA7Z21kA4BEJEAAAAASUVORK5CYII=',
  },
  theme: {
    mode: 'light',
  },
  widget: {
    languageToggle: false,
  },
  tabbar: {
    persist: false,
  },
});
