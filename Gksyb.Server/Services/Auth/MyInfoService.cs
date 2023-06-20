using Gksyb.Core.Auth;
using Gksyb.Model.Core;

namespace Gksyb.Server.Services.Auth
{
    public class MyInfoService : IBaseService
    {
        private readonly string _customColumn = "定制列";
        private readonly IDbContext _dbContext;
        private readonly UserSession _user;

        public MyInfoService(IDbContext dbContext, UserSession user)
        {
            _dbContext = dbContext;
            _user = user;
            _dbContext.DisableSqlLog();
        }

        /// <summary>
        /// 记录菜单点击
        /// </summary>
        /// <returns></returns>
        public async Task MenuClickAsync(string menuNo, string appname)
        {
            if (string.IsNullOrWhiteSpace(appname))
            {
                appname = _user.MenuAppname;
            }
            if (string.IsNullOrWhiteSpace(appname))
            {
                appname = _user.UserAppName;
            }
            var sysdate = await _dbContext.GetSysdate();
            var month = sysdate.Value.ToString("yyyy-MM");
            var favorite = await _dbContext.Query<CF_FAVORITE_TJ>().Where(c => c.USERID == _user.UserID &&
             c.FAVORITETITLE == menuNo && c.TJ_MONTH == month && c.APPNAME == appname).FirstOrDefaultAsync();
            if (favorite == null)
            {
                var menu = await _dbContext.Query<SYS_MENU>().Where(c => c.MENUNO == menuNo && c.APPNAME == appname).FirstOrDefaultAsync();
                if (menu == null) return;
                menu.MENUURL ??= "";
                favorite = new CF_FAVORITE_TJ()
                {
                    CLICKID = GuidHelper.NewSnowflakeId(),
                    USERID = _user.UserID,
                    USERNAME = _user.Display,
                    FAVORITETITLE = menuNo,
                    APPNAME = appname,
                    FAVORITECONTENT = menu.MENUNAME,
                    URL = string.Concat(menu.MENUURL, menu.MENUURL.Contains('?') ? "&" : "?", "MenuNo=", menu.MENUNO),
                    ICON = menu.MENUICON,
                    CLICKNUM = 1,
                    TJ_MONTH = month
                };
                await _dbContext.InsertAsync(favorite);
            }
            else
            {
                await _dbContext.UpdateAsync<CF_FAVORITE_TJ>(c => c.CLICKID == favorite.CLICKID, c => new CF_FAVORITE_TJ()
                {
                    CLICKNUM = c.CLICKNUM + 1
                });
            }
            await _dbContext.UserLogAsync("菜单点击", favorite.FAVORITECONTENT, $"账号【{_user.Display}】访问【{favorite.FAVORITECONTENT}】，菜单编号{appname}__{menuNo}", _user);
        }

        /// <summary>
        /// 自定义列
        /// </summary>
        /// <returns></returns>
        public async Task<string> CustomColumnAsync(string id, string appname)
        {
            if (string.IsNullOrWhiteSpace(appname))
            {
                appname = _user.MenuAppname;
            }
            if (string.IsNullOrWhiteSpace(appname))
            {
                appname = _user.UserAppName;
            }
            var userId = _user.UserID.ToString();
            var column = await _dbContext.Query<CF_USERPRIVILEGE>()
                .Where(c => c.PRIVILEGEACCESS == id && c.PRIVILEGEMASTER == _customColumn && c.PRIVILEGEMASTERKEY == userId && c.APPNAME == appname)
                .Select(c => c.PRIVILEGEACCESSKEY).FirstOrDefaultAsync();
            return column;
        }

        /// <summary>
        /// 保存自定义列
        /// </summary>
        /// <returns></returns>
        public async Task CustomColumnSaveAsync(string id, string columns, string appname)
        {
            if (string.IsNullOrWhiteSpace(appname))
            {
                appname = _user.MenuAppname;
            }
            if (string.IsNullOrWhiteSpace(appname))
            {
                appname = _user.UserAppName;
            }
            var userId = _user.UserID.ToString();
            if (string.IsNullOrWhiteSpace(columns))
            {
                await _dbContext.DeleteAsync<CF_USERPRIVILEGE>(c => c.PRIVILEGEACCESS == id && c.PRIVILEGEMASTER == _customColumn && c.PRIVILEGEMASTERKEY == userId && c.APPNAME == appname);
                return;
            }
            var model = await _dbContext.Query<CF_USERPRIVILEGE>()
                .Where(c => c.PRIVILEGEACCESS == id && c.PRIVILEGEMASTER == _customColumn && c.PRIVILEGEMASTERKEY == userId && c.APPNAME == appname)
                .FirstOrDefaultAsync();
            if (model == null)
            {
                model = new CF_USERPRIVILEGE()
                {
                    PRIVILEGEACCESS = id,
                    PRIVILEGEACCESSKEY = columns,
                    PRIVILEGEMASTER = _customColumn,
                    PRIVILEGEMASTERKEY = userId,
                    APPNAME = appname
                };
                await _dbContext.InsertAsync(model);
            }
            else
            {
                await _dbContext.UpdateAsync<CF_USERPRIVILEGE>(c => c.PRIVILEGEACCESS == id && c.PRIVILEGEMASTER == _customColumn && c.PRIVILEGEMASTERKEY == userId && c.APPNAME == appname, c => new CF_USERPRIVILEGE()
                {
                    PRIVILEGEACCESSKEY = columns
                });
            }
            await _dbContext.UserLogAsync(_customColumn, id, $"账号【{_user.Display}】{_customColumn}【{columns}】，编号{id}", _user); ;
        }
    }
}